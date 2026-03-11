namespace CycleTrust.Core.Entities;

public class ChatConversation : BaseEntity
{
    public long? ListingId { get; set; }
    public long BuyerId { get; set; }
    public long SellerId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessage { get; set; }
    public long? LastMessageSenderId { get; set; }
    public int UnreadCountBuyer { get; set; } = 0;
    public int UnreadCountSeller { get; set; } = 0;
    
    public Listing? Listing { get; set; }
    public User Buyer { get; set; } = null!;
    public User Seller { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
