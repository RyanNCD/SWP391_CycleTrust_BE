namespace CycleTrust.Core.Entities;

public class Review : BaseEntity
{
    public long OrderId { get; set; }
    public long BuyerId { get; set; }
    public long SellerId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    
    public Order Order { get; set; } = null!;
    public User Buyer { get; set; } = null!;
    public User Seller { get; set; } = null!;
}
