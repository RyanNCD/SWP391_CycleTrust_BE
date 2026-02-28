namespace CycleTrust.Application.DTOs.Order;

public class OrderDto
{
    public long Id { get; set; }
    public long ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public long BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public long SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long PriceAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public bool DepositRequired { get; set; }
    public long DepositAmount { get; set; }
    public DateTime? DepositDueAt { get; set; }
    public DateTime? DepositPaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PaymentDto> Payments { get; set; } = new();
}

public class PaymentDto
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string? Provider { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOrderRequest
{
    public long ListingId { get; set; }
    public bool DepositRequired { get; set; } = true;
    public string? ShippingNote { get; set; }
}

public class PaymentRequest
{
    public long OrderId { get; set; }
    public string PaymentType { get; set; } = "DEPOSIT";
    public string Provider { get; set; } = "mock";
}

public class PaymentCallbackRequest
{
    public long PaymentId { get; set; }
    public string Status { get; set; } = "PAID";
    public string? ProviderTxnId { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
}
