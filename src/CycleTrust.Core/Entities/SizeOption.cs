namespace CycleTrust.Core.Entities;

public class SizeOption : BaseEntity
{
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
