using CycleTrust.Application.DTOs.Notification;

namespace CycleTrust.Application.Services;

public interface INotificationBroadcaster
{
    Task BroadcastNotificationAsync(long userId, NotificationDto notification);
}
