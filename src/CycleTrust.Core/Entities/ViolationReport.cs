using CycleTrust.Core.Enums;

namespace CycleTrust.Core.Entities;

public class ViolationReport : BaseEntity
{
    public long ReporterId { get; set; }
    public long? ListingId { get; set; }
    public long? ReportedUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.OPEN;
    public long? HandledBy { get; set; }
    public DateTime? HandledAt { get; set; }
    public string? ResolutionNote { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public User Reporter { get; set; } = null!;
    public Listing? Listing { get; set; }
    public User? ReportedUser { get; set; }
    public User? Handler { get; set; }
}
