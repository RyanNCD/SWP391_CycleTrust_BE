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
            await _hubContext.Clients
                .Group($"conversation_{conversationId}")
                .SendAsync("ReceiveMessage", message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to broadcast message: {ex.Message}");
        }
    }
}
