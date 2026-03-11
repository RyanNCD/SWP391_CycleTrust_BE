namespace CycleTrust.Core.Entities;

public class ChatMessage : BaseEntity
{
    public long ConversationId { get; set; }
    public long SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    
    public ChatConversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
