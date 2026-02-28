namespace CycleTrust.Core.Enums;

public enum UserRole
{
    BUYER,
    SELLER,
    ADMIN,
    INSPECTOR
}

public enum ListingStatus
{
    DRAFT,
    PENDING_APPROVAL,
    APPROVED,
    REJECTED,
    UNDER_INSPECTION,
    VERIFIED,
    SOLD,
    ARCHIVED
}

public enum MediaType
{
    IMAGE,
    VIDEO
}

public enum DepositMode
{
    PERCENT,
    FIXED
}

public enum OrderStatus
{
    PLACED,
    DEPOSIT_PENDING,
    DEPOSIT_PAID,
    CONFIRMED,
    SHIPPING,
    DELIVERED,
    COMPLETED,
    CANCELED,
    DISPUTED
}

public enum PaymentType
{
    DEPOSIT,
    FULL,
    REFUND
}

public enum PaymentStatus
{
    PENDING,
    PAID,
    FAILED,
    REFUNDED
}

public enum ReportStatus
{
    OPEN,
    IN_REVIEW,
    RESOLVED,
    REJECTED
}

public enum DisputeStatus
{
    OPEN,
    ASSIGNED,
    IN_PROGRESS,
    RESOLVED,
    CLOSED
}
