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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
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
    public string InspectorName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? ChecklistJson { get; set; }
    public string? ReportUrl { get; set; }
    public DateTime CreatedAt { get; set; }
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
