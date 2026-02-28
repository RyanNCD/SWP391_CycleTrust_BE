using CycleTrust.Core.Enums;

namespace CycleTrust.Core.Entities;

public class Listing : BaseEntity
{
    public long SellerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UsageHistory { get; set; }
    public string? LocationText { get; set; }
    
    public long? BrandId { get; set; }
    public long? CategoryId { get; set; }
    public long? SizeOptionId { get; set; }
    
    public long PriceAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? ConditionNote { get; set; }
    public int? YearModel { get; set; }
    
    public ListingStatus Status { get; set; } = ListingStatus.DRAFT;
    public long? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectedReason { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public User Seller { get; set; } = null!;
    public Brand? Brand { get; set; }
    public BikeCategory? Category { get; set; }
    public SizeOption? SizeOption { get; set; }
    public User? ApprovedByUser { get; set; }
    public ICollection<ListingMedia> Media { get; set; } = new List<ListingMedia>();
    public Inspection? Inspection { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
