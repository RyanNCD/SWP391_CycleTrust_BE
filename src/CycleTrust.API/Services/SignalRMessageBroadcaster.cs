using Microsoft.AspNetCore.SignalR;
using CycleTrust.Application.DTOs.Chat;
using CycleTrust.Application.Services;
using CycleTrust.API.Hubs;

namespace CycleTrust.API.Services;

public class SignalRMessageBroadcaster : IMessageBroadcaster
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SignalRMessageBroadcaster(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastMessageAsync(long conversationId, ChatMessageDto message)
    {
        try
        {
            Console.WriteLine($"[SignalR] Broadcasting message to conversation_{conversationId}");
            Console.WriteLine($"[SignalR] Message: {message.Content}");
            Console.WriteLine($"[SignalR] Sender: {message.SenderName}");
            
            await _hubContext.Clients
                .Group($"conversation_{conversationId}")
                .SendAsync("ReceiveMessage", message);
                
            Console.WriteLine($"[SignalR] Message broadcast completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Failed to broadcast message: {ex.Message}");
        }
    }
}
