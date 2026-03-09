using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.Notification;

namespace CycleTrust.Application.Services;

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationRequest request);
    Task<List<NotificationDto>> GetUserNotificationsAsync(long userId, int limit = 20);
    Task<NotificationSummaryDto> GetNotificationSummaryAsync(long userId);
    Task<NotificationDto> MarkAsReadAsync(long notificationId, long userId);
    Task MarkAllAsReadAsync(long userId);
    Task DeleteNotificationAsync(long notificationId, long userId);
}

public class NotificationService : INotificationService
{
    private readonly CycleTrustDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationBroadcaster _notificationBroadcaster;

    public NotificationService(
        CycleTrustDbContext context, 
        IMapper mapper,
        INotificationBroadcaster notificationBroadcaster)
    {
        _context = context;
        _mapper = mapper;
        _notificationBroadcaster = notificationBroadcaster;
    }

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationRequest request)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            RelatedEntityId = request.RelatedEntityId,
            RelatedEntityType = request.RelatedEntityType,
            ActionUrl = request.ActionUrl,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var notificationDto = _mapper.Map<NotificationDto>(notification);
        Console.WriteLine($"[NotificationService] Created notification ID {notificationDto.Id} for user {request.UserId}");

        // Broadcast notification via SignalR
        try
        {
            Console.WriteLine($"[NotificationService] Broadcasting notification to user {request.UserId}");
            await _notificationBroadcaster.BroadcastNotificationAsync(request.UserId, notificationDto);
            Console.WriteLine($"[NotificationService] Notification broadcast completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationService] Broadcast failed: {ex.Message}");
        }

        return notificationDto;
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(long userId, int limit = 20)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(long userId)
    {
        var unreadCount = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();

        var recentNotifications = await GetUserNotificationsAsync(userId, 5);

        return new NotificationSummaryDto
        {
            UnreadCount = unreadCount,
            RecentNotifications = recentNotifications
        };
    }

    public async Task<NotificationDto> MarkAsReadAsync(long notificationId, long userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
            throw new Exception("Notification không tồn tại");

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return _mapper.Map<NotificationDto>(notification);
    }

    public async Task MarkAllAsReadAsync(long userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteNotificationAsync(long notificationId, long userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
            throw new Exception("Notification không tồn tại");

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
    }
}
