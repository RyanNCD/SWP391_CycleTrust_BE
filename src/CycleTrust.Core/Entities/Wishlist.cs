namespace CycleTrust.Core.Entities;

public class Wishlist
{
    public long BuyerId { get; set; }
    public long ListingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public User Buyer { get; set; } = null!;
    public Listing Listing { get; set; } = null!;
}
