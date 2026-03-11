using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.Chat;

namespace CycleTrust.Application.Services;

public interface IChatService
{
    Task<ChatConversationDto> GetOrCreateConversationAsync(long userId, long listingId, long sellerId);
    Task<List<ChatConversationDto>> GetUserConversationsAsync(long userId);
    Task<List<ChatMessageDto>> GetConversationMessagesAsync(long conversationId, long userId, int limit = 50);
    Task<ChatMessageDto> SendMessageAsync(long senderId, SendMessageRequest request);
    Task MarkMessagesAsReadAsync(long conversationId, long userId);
}

public class ChatService : IChatService
{
    private readonly CycleTrustDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IMessageBroadcaster _messageBroadcaster;

    public ChatService(
        CycleTrustDbContext context,
        IMapper mapper,
        INotificationService notificationService,
        IMessageBroadcaster messageBroadcaster)
    {
        _context = context;
        _mapper = mapper;
        _notificationService = notificationService;
        _messageBroadcaster = messageBroadcaster;
    }

    public async Task<ChatConversationDto> GetOrCreateConversationAsync(long userId, long listingId, long sellerId)
    {
        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId);

        if (listing == null)
            throw new Exception("Listing không tồn tại");

        var buyerId = userId;

        var conversation = await _context.ChatConversations
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.Listing)
            .FirstOrDefaultAsync(c => c.ListingId == listingId && c.BuyerId == buyerId && c.SellerId == sellerId);

        if (conversation == null)
        {
            conversation = new ChatConversation
            {
                ListingId = listingId,
                BuyerId = buyerId,
                SellerId = sellerId
            };

            _context.ChatConversations.Add(conversation);
            await _context.SaveChangesAsync();

            conversation = await _context.ChatConversations
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Listing)
                .FirstAsync(c => c.Id == conversation.Id);
        }

        return MapToConversationDto(conversation, userId);
    }

    public async Task<List<ChatConversationDto>> GetUserConversationsAsync(long userId)
    {
        var conversations = await _context.ChatConversations
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.Listing)
            .Where(c => c.BuyerId == userId || c.SellerId == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync();

        return conversations.Select(c => MapToConversationDto(c, userId)).ToList();
    }

    public async Task<List<ChatMessageDto>> GetConversationMessagesAsync(long conversationId, long userId, int limit = 50)
    {
        var conversation = await _context.ChatConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && (c.BuyerId == userId || c.SellerId == userId));

        if (conversation == null)
            throw new Exception("Conversation không tồn tại hoặc bạn không có quyền truy cập");

        var messages = await _context.ChatMessages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync();

        messages.Reverse();

        return messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderId = m.SenderId,
            SenderName = m.Sender.FullName,
            SenderAvatar = m.Sender.AvatarUrl,
            Content = m.Content,
            IsRead = m.IsRead,
            ReadAt = m.ReadAt,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<ChatMessageDto> SendMessageAsync(long senderId, SendMessageRequest request)
    {
        ChatConversation? conversation = null;

        if (request.ConversationId.HasValue)
        {
            conversation = await _context.ChatConversations
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value);

            if (conversation == null)
                throw new Exception("Conversation không tồn tại");

            if (conversation.BuyerId != senderId && conversation.SellerId != senderId)
                throw new Exception("Bạn không có quyền gửi tin nhắn trong conversation này");
        }
        else if (request.ListingId.HasValue)
        {
            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.Id == request.ListingId.Value);

            if (listing == null)
                throw new Exception("Listing không tồn tại");

            conversation = await GetOrCreateConversationEntityAsync(senderId, request.ListingId.Value, listing.SellerId);
        }
        else
        {
            throw new Exception("Phải cung cấp ConversationId hoặc ListingId");
        }

        if (conversation == null)
        {
            throw new Exception("Không thể tạo hoặc tìm thấy conversation");
        }

        var message = new ChatMessage
        {
            ConversationId = conversation.Id,
            SenderId = senderId,
            Content = request.Content,
            IsRead = false
        };

        _context.ChatMessages.Add(message);

        conversation.LastMessage = request.Content.Length > 100 ? request.Content.Substring(0, 100) + "..." : request.Content;
        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.LastMessageSenderId = senderId;

        if (conversation.BuyerId == senderId)
        {
            conversation.UnreadCountSeller++;
        }
        else
        {
            conversation.UnreadCountBuyer++;
        }

        await _context.SaveChangesAsync();

        message = await _context.ChatMessages
            .Include(m => m.Sender)
            .FirstAsync(m => m.Id == message.Id);

        var messageDto = new ChatMessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderName = message.Sender.FullName,
            SenderAvatar = message.Sender.AvatarUrl,
            Content = message.Content,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            CreatedAt = message.CreatedAt
        };

        long receiverId = conversation.BuyerId == senderId ? conversation.SellerId : conversation.BuyerId;

        try
        {
            Console.WriteLine($"[ChatService] Creating notification for user {receiverId}");
            await _notificationService.CreateNotificationAsync(new DTOs.Notification.CreateNotificationRequest
            {
                UserId = receiverId,
                Type = Core.Enums.NotificationType.MESSAGE_RECEIVED,
                Title = "Tin nhắn mới",
                Message = $"{message.Sender.FullName}: {(message.Content.Length > 50 ? message.Content.Substring(0, 50) + "..." : message.Content)}",
                RelatedEntityId = conversation.Id,
                RelatedEntityType = "ChatConversation",
                ActionUrl = $"/chat/{conversation.Id}"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatService] Notification creation failed: {ex.Message}");
        }

        try
        {
            Console.WriteLine($"[ChatService] Broadcasting message to conversation {conversation.Id}");
            await _messageBroadcaster.BroadcastMessageAsync(conversation.Id, messageDto);
            Console.WriteLine($"[ChatService] Message broadcast completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatService] Broadcast failed: {ex.Message}");
        }

        return messageDto;
    }

    public async Task MarkMessagesAsReadAsync(long conversationId, long userId)
    {
        var conversation = await _context.ChatConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && (c.BuyerId == userId || c.SellerId == userId));

        if (conversation == null)
            throw new Exception("Conversation không tồn tại");

        var messages = await _context.ChatMessages
            .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        if (conversation.BuyerId == userId)
        {
            conversation.UnreadCountBuyer = 0;
        }
        else
        {
            conversation.UnreadCountSeller = 0;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<ChatConversation> GetOrCreateConversationEntityAsync(long buyerId, long listingId, long sellerId)
    {
        var conversation = await _context.ChatConversations
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.Listing)
            .FirstOrDefaultAsync(c => c.ListingId == listingId && c.BuyerId == buyerId && c.SellerId == sellerId);

        if (conversation == null)
        {
            conversation = new ChatConversation
            {
                ListingId = listingId,
                BuyerId = buyerId,
                SellerId = sellerId
            };

            _context.ChatConversations.Add(conversation);
            await _context.SaveChangesAsync();

            conversation = await _context.ChatConversations
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Listing)
                .FirstAsync(c => c.Id == conversation.Id);
        }

        return conversation;
    }

    private ChatConversationDto MapToConversationDto(ChatConversation conversation, long currentUserId)
    {
        bool isBuyer = conversation.BuyerId == currentUserId;
        var otherUser = isBuyer ? conversation.Seller : conversation.Buyer;

        return new ChatConversationDto
        {
            Id = conversation.Id,
            ListingId = conversation.ListingId,
            ListingTitle = conversation.Listing?.Title,
            BuyerId = conversation.BuyerId,
            BuyerName = conversation.Buyer.FullName,
            BuyerAvatar = conversation.Buyer.AvatarUrl,
            SellerId = conversation.SellerId,
            SellerName = conversation.Seller.FullName,
            SellerAvatar = conversation.Seller.AvatarUrl,
            LastMessageAt = conversation.LastMessageAt,
            LastMessage = conversation.LastMessage,
            LastMessageSenderId = conversation.LastMessageSenderId,
            UnreadCountBuyer = conversation.UnreadCountBuyer,
            UnreadCountSeller = conversation.UnreadCountSeller,
            CreatedAt = conversation.CreatedAt
        };
    }
}
