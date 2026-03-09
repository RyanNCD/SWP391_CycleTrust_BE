using Microsoft.AspNetCore.SignalR;
using CycleTrust.Application.DTOs.Notification;
using CycleTrust.Application.Services;
using CycleTrust.API.Hubs;

namespace CycleTrust.API.Services;

public class SignalRNotificationBroadcaster : INotificationBroadcaster
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationBroadcaster(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastNotificationAsync(long userId, NotificationDto notification)
    {
        try
        {
            Console.WriteLine($"[SignalR] Broadcasting notification to user_{userId}");
            Console.WriteLine($"[SignalR] Notification: {notification.Title}");
            
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification", notification);
                
            Console.WriteLine($"[SignalR] Notification broadcast completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Failed to broadcast notification: {ex.Message}");
        }
    }
}
