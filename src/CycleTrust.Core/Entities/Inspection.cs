namespace CycleTrust.Core.Entities;

public class Inspection : BaseEntity
{
    public long ListingId { get; set; }
    public long InspectorId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? ChecklistJson { get; set; }
    public string? ReportUrl { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public Listing Listing { get; set; } = null!;
    public User Inspector { get; set; } = null!;
}
