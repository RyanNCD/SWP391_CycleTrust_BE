using CycleTrust.Core.Enums;

namespace CycleTrust.Core.Entities;

public class ListingMedia : BaseEntity
{
    public long ListingId { get; set; }
    public MediaType Type { get; set; }
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    
    public Listing Listing { get; set; } = null!;
}
