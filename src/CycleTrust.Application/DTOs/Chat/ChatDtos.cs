namespace CycleTrust.Application.DTOs.Chat;

public class ChatConversationDto
{
    public long Id { get; set; }
    public long? ListingId { get; set; }
    public string? ListingTitle { get; set; }
    public long BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string? BuyerAvatar { get; set; }
    public long SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? SellerAvatar { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessage { get; set; }
    public long? LastMessageSenderId { get; set; }
    public int UnreadCountBuyer { get; set; }
    public int UnreadCountSeller { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ChatMessageDto
{
    public long Id { get; set; }
    public long ConversationId { get; set; }
    public long SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendMessageRequest
{
    public long? ConversationId { get; set; }
    public long? ListingId { get; set; }
  public long ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class GetOrCreateConversationRequest
{
    public long ListingId { get; set; }
    public long SellerId { get; set; }
}

public class MarkMessagesReadRequest
{
    public long ConversationId { get; set; }
}
