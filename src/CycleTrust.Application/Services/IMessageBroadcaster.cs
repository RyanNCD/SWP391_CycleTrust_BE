using CycleTrust.Application.DTOs.Chat;

namespace CycleTrust.Application.Services;

public interface IMessageBroadcaster
{
    Task BroadcastMessageAsync(long conversationId, ChatMessageDto message);
}
