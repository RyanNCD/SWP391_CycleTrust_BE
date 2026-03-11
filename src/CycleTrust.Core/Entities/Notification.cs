using CycleTrust.Core.Enums;

namespace CycleTrust.Core.Entities;

public class Notification : BaseEntity
{
    public long UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public long? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; } // "Order", "Listing", "Dispute", etc.
    public string? ActionUrl { get; set; }
    
    public User User { get; set; } = null!;
}
