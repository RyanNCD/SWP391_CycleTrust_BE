namespace CycleTrust.Application.DTOs.Wishlist;

public class WishlistItemDto
{
    public long ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public long PriceAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? MainImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}
