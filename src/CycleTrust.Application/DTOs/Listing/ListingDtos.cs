using CycleTrust.Application.DTOs.User;
using CycleTrust.Application.DTOs.Catalog;

namespace CycleTrust.Application.DTOs.Listing;

public class ListingDto
{
    public long Id { get; set; }
    public long SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UsageHistory { get; set; }
    public string? LocationText { get; set; }
    public long? BrandId { get; set; }
    public string? BrandName { get; set; }
    public long? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public long? SizeOptionId { get; set; }
    public string? SizeLabel { get; set; }
    public long PriceAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? ConditionNote { get; set; }
    public int? YearModel { get; set; }
    public string Status { get; set; } = string.Empty;
    public long? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectedReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserDto? Seller { get; set; }
    public BrandDto? Brand { get; set; }
    public CategoryDto? Category { get; set; }
    public SizeOptionDto? SizeOption { get; set; }
    public List<ListingMediaDto> Media { get; set; } = new();
    public InspectionDto? Inspection { get; set; }
}

public class ListingMediaDto
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class InspectionDto
{
    public long Id { get; set; }
    public long InspectorId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? ChecklistJson { get; set; }
    public string? ReportUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserDto? Inspector { get; set; }
}

public class CreateListingRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UsageHistory { get; set; }
    public string? LocationText { get; set; }
    public long? BrandId { get; set; }
    public long? CategoryId { get; set; }
    public long? SizeOptionId { get; set; }
    public long PriceAmount { get; set; }
    public string? ConditionNote { get; set; }
    public int? YearModel { get; set; }
    public List<CreateListingMediaRequest> Media { get; set; } = new();
    public string? Status { get; set; } // "DRAFT" or "PENDING_APPROVAL"
}

public class CreateListingMediaRequest
{
    public string Type { get; set; } = "IMAGE";
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class UpdateListingRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? UsageHistory { get; set; }
    public string? LocationText { get; set; }
    public long? BrandId { get; set; }
    public long? CategoryId { get; set; }
    public long? SizeOptionId { get; set; }
    public long? PriceAmount { get; set; }
    public string? ConditionNote { get; set; }
    public int? YearModel { get; set; }
    public List<CreateListingMediaRequest>? Media { get; set; }
    public string? Status { get; set; } // "DRAFT" or "PENDING_APPROVAL"
}

public class ApproveListingRequest
{
    public bool Approved { get; set; }
    public string? Reason { get; set; }
}

public class CreateInspectionRequest
{
    public string Summary { get; set; } = string.Empty;
    public string? ChecklistJson { get; set; }
    public string? ReportUrl { get; set; }
}
