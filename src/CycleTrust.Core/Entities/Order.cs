using CycleTrust.Core.Enums;

namespace CycleTrust.Core.Entities;

public class Order : BaseEntity
{
    public long ListingId { get; set; }
    public long BuyerId { get; set; }
    public long SellerId { get; set; }
    
    public OrderStatus Status { get; set; } = OrderStatus.PLACED;
    
    public long PriceAmount { get; set; }
    public string Currency { get; set; } = "VND";
    
    public bool DepositRequired { get; set; } = true;
    public long DepositAmount { get; set; } = 0;
    public DateTime? DepositDueAt { get; set; }
    public DateTime? DepositPaidAt { get; set; }
    public DateTime? ReserveExpiresAt { get; set; }
    
    public string? ShippingNote { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CanceledReason { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public Listing Listing { get; set; } = null!;
    public User Buyer { get; set; } = null!;
    public User Seller { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Review? Review { get; set; }
    public Dispute? Dispute { get; set; }
}
