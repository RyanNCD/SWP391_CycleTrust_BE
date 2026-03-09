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

public enum NotificationType
{
    ORDER_CREATED,
    ORDER_CONFIRMED,
    ORDER_SHIPPING,
    ORDER_DELIVERED,
    ORDER_COMPLETED,
    ORDER_CANCELED,
    PAYMENT_SUCCESS,
    PAYMENT_FAILED,
    LISTING_APPROVED,
    LISTING_REJECTED,
    LISTING_VERIFIED,
    INSPECTION_COMPLETED,
    DISPUTE_CREATED,
    DISPUTE_RESOLVED,
    REVIEW_RECEIVED,
    MESSAGE_RECEIVED,
    SELLER_APPROVED,
    SELLER_REJECTED
}
