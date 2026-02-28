using CycleTrust.Core.Enums;

namespace CycleTrust.Core.Entities;

public class Payment : BaseEntity
{
    public long OrderId { get; set; }
    public PaymentType Type { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;
    public long Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string? Provider { get; set; }
    public string? ProviderTxnId { get; set; }
    public DateTime? PaidAt { get; set; }
    
    public Order Order { get; set; } = null!;
}
