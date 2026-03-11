using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.Order;
using CycleTrust.Application.DTOs.Notification;

namespace CycleTrust.Application.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(long buyerId, CreateOrderRequest request);
    Task<OrderDto> GetOrderByIdAsync(long id);
    Task<List<OrderDto>> GetMyOrdersAsync(long userId, string role);
    Task<List<OrderDto>> GetAllOrdersForAdminAsync(string? status, DateTime? fromDate, DateTime? toDate);
    Task<PaymentDto> CreatePaymentAsync(long userId, PaymentRequest request);
    Task<PaymentDto> CreateRemainingPaymentAsync(long userId, long orderId, PaymentRequest request);
    Task<PaymentDto> ProcessPaymentCallbackAsync(PaymentCallbackRequest request);
    Task<OrderDto> UpdateOrderStatusAsync(long id, long userId, UpdateOrderStatusRequest request);
}

public class OrderService : IOrderService
{
    private readonly CycleTrustDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public OrderService(CycleTrustDbContext context, IMapper mapper, INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<OrderDto> CreateOrderAsync(long buyerId, CreateOrderRequest request)
    {
        var listing = await _context.Listings
            .Include(l => l.Seller)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId && !l.IsDeleted);

        if (listing == null)
            throw new Exception("Listing không tồn tại");

        if (listing.Status != ListingStatus.APPROVED && listing.Status != ListingStatus.VERIFIED)
            throw new Exception("Listing chưa được duyệt hoặc chưa sẵn sàng để mua");

        if (listing.SellerId == buyerId)
            throw new Exception("Không thể mua listing của chính mình");

        var activeOrder = await _context.Orders
            .AnyAsync(o => o.ListingId == request.ListingId &&
                          (o.Status == OrderStatus.PLACED || o.Status == OrderStatus.DEPOSIT_PENDING ||
                           o.Status == OrderStatus.DEPOSIT_PAID || o.Status == OrderStatus.CONFIRMED ||
                           o.Status == OrderStatus.SHIPPING || o.Status == OrderStatus.DISPUTED));

        if (activeOrder)
            throw new Exception("Listing đã có người đặt mua");

        long depositAmount = 0;
        if (request.DepositRequired)
        {
            var policy = await _context.DepositPolicies
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (policy != null)
            {
                if (policy.Mode == DepositMode.PERCENT && policy.PercentValue.HasValue)
                {
                    depositAmount = (long)(listing.PriceAmount * (policy.PercentValue.Value / 100));
                }
                else if (policy.Mode == DepositMode.FIXED && policy.FixedAmount.HasValue)
                {
                    depositAmount = policy.FixedAmount.Value;
                }

                if (depositAmount < policy.MinAmount)
                    depositAmount = policy.MinAmount;
                if (policy.MaxAmount.HasValue && depositAmount > policy.MaxAmount.Value)
                    depositAmount = policy.MaxAmount.Value;
            }
            else
            {
                depositAmount = (long)(listing.PriceAmount * 0.1m);
            }
        }

        var order = new Order
        {
            ListingId = listing.Id,
            BuyerId = buyerId,
            SellerId = listing.SellerId,
            Status = request.DepositRequired ? OrderStatus.DEPOSIT_PENDING : OrderStatus.PLACED,
            PriceAmount = listing.PriceAmount,
            Currency = listing.Currency,
            DepositRequired = request.DepositRequired,
            DepositAmount = depositAmount,
            DepositDueAt = request.DepositRequired ? DateTime.UtcNow.AddDays(3) : null,
            ShippingNote = request.ShippingNote
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        try
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                UserId = listing.SellerId,
                Type = NotificationType.ORDER_CREATED,
                Title = "Đơn hàng mới",
                Message = $"Bạn có đơn hàng mới cho xe {listing.Title}",
                RelatedEntityId = order.Id,
                RelatedEntityType = "Order",
                ActionUrl = $"/seller/orders/{order.Id}"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send ORDER_CREATED notification: {ex.Message}");
        }

        return await GetOrderByIdAsync(order.Id);
    }

    public async Task<OrderDto> GetOrderByIdAsync(long id)
    {
        var order = await _context.Orders
            .Include(o => o.Listing)
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            throw new Exception("Order không tồn tại");

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync(long userId, string role)
    {
        var query = _context.Orders
            .Include(o => o.Listing)
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Include(o => o.Payments)
            .AsQueryable();

        if (role == "BUYER")
            query = query.Where(o => o.BuyerId == userId);
        else if (role == "SELLER")
            query = query.Where(o => o.SellerId == userId);

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<List<OrderDto>> GetAllOrdersForAdminAsync(string? status, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.Orders
            .Include(o => o.Listing)
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .Include(o => o.Payments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
        {
            query = query.Where(o => o.Status == orderStatus);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= toDate.Value);
        }

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<PaymentDto> CreatePaymentAsync(long userId, PaymentRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.BuyerId == userId);

        if (order == null)
            throw new Exception("Order không tồn tại hoặc không có quyền");

        var paymentType = Enum.Parse<PaymentType>(request.PaymentType, true);
        long amount = 0;

        if (paymentType == PaymentType.DEPOSIT)
        {
            if (!order.DepositRequired)
                throw new Exception("Order không yêu cầu đặt cọc");
            if (order.Status != OrderStatus.DEPOSIT_PENDING)
                throw new Exception("Order không ở trạng thái DEPOSIT_PENDING");
            amount = order.DepositAmount;
        }
        else if (paymentType == PaymentType.FULL)
        {
            if (order.DepositRequired)
            {
                var depositPaid = order.Payments.Any(p => p.Type == PaymentType.DEPOSIT && p.Status == PaymentStatus.PAID);
                if (!depositPaid)
                    throw new Exception("Chưa thanh toán cọc");
                amount = order.PriceAmount - order.DepositAmount;
            }
            else
            {
                amount = order.PriceAmount;
            }
        }

        var payment = new Payment
        {
            OrderId = order.Id,
            Type = paymentType,
            Status = PaymentStatus.PENDING,
            Amount = amount,
            Currency = order.Currency,
            Provider = request.Provider
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CreateRemainingPaymentAsync(long userId, long orderId, PaymentRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.Payments)
            .Include(o => o.Listing)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BuyerId == userId);

        if (order == null)
            throw new Exception("Order không tồn tại hoặc không có quyền");

        if (order.Status != OrderStatus.DELIVERED)
            throw new Exception("Order chưa được giao hàng");

        if (!order.DepositRequired || !order.DepositPaidAt.HasValue)
            throw new Exception("Order không phải là order đặt cọc");

        var fullPaymentExists = order.Payments.Any(p => p.Type == PaymentType.FULL && p.Status == PaymentStatus.PAID);
        if (fullPaymentExists)
            throw new Exception("Đã thanh toán phần còn lại");

        var remainingAmount = order.PriceAmount - order.DepositAmount;

        var payment = new Payment
        {
            OrderId = order.Id,
            Type = PaymentType.FULL,
            Status = PaymentStatus.PENDING,
            Amount = remainingAmount,
            Currency = order.Currency,
            Provider = request.Provider
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> ProcessPaymentCallbackAsync(PaymentCallbackRequest request)
    {
        Console.WriteLine($"========== CALLBACK START ==========");
        Console.WriteLine($"Request PaymentId: {request.PaymentId}");
        Console.WriteLine($"Request Status: {request.Status}");
        Console.WriteLine($"Request ProviderTxnId: {request.ProviderTxnId}");
        
        var payment = await _context.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.Listing)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId);

        if (payment == null)
            throw new Exception("Payment không tồn tại");

        Console.WriteLine($"Payment found - ID: {payment.Id}, Type: {payment.Type}, Current Status: {payment.Status}");
        Console.WriteLine($"Order ID: {payment.OrderId}");

        var status = Enum.Parse<PaymentStatus>(request.Status, true);
        payment.Status = status;
        payment.ProviderTxnId = request.ProviderTxnId;

        Console.WriteLine($"Parsed Status: {status}");
        Console.WriteLine($"Status == PAID: {status == PaymentStatus.PAID}");

        if (status == PaymentStatus.PAID)
        {
            Console.WriteLine(">>> Inside PAID block");
            payment.PaidAt = DateTime.UtcNow;

            Console.WriteLine($"Payment Type: {payment.Type}");
            Console.WriteLine($"Is DEPOSIT: {payment.Type == PaymentType.DEPOSIT}");
            Console.WriteLine($"Is FULL: {payment.Type == PaymentType.FULL}");

            if (payment.Type == PaymentType.DEPOSIT)
            {
                Console.WriteLine(">>> Processing DEPOSIT payment");
                payment.Order.Status = OrderStatus.DEPOSIT_PAID;
                payment.Order.DepositPaidAt = DateTime.UtcNow;
                payment.Order.ReserveExpiresAt = DateTime.UtcNow.AddDays(7);
            }
            else if (payment.Type == PaymentType.FULL)
            {
                Console.WriteLine($"========== PAYMENT CALLBACK DEBUG ==========");
                Console.WriteLine($"Payment ID: {payment.Id}");
                Console.WriteLine($"Order ID: {payment.Order.Id}");
                Console.WriteLine($"Order Status: {payment.Order.Status}");
                Console.WriteLine($"Order DeliveredAt: {payment.Order.DeliveredAt?.ToString() ?? "NULL"}");
                Console.WriteLine($"Order DepositPaidAt: {payment.Order.DepositPaidAt?.ToString() ?? "NULL"}");
                Console.WriteLine($"Has DeliveredAt: {payment.Order.DeliveredAt.HasValue}");
                Console.WriteLine($"==========================================");
                
                if (payment.Order.DeliveredAt.HasValue)
                {
                    Console.WriteLine(">>> SETTING ORDER TO COMPLETED");
                    payment.Order.Status = OrderStatus.COMPLETED;
                    payment.Order.CompletedAt = DateTime.UtcNow;
                    payment.Order.Listing.Status = ListingStatus.SOLD;
                    
                    try
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                        {
                            UserId = payment.Order.SellerId,
                            Type = NotificationType.ORDER_COMPLETED,
                            Title = "Đơn hàng hoàn thành",
                            Message = $"Đơn hàng xe {payment.Order.Listing.Title} đã hoàn thành",
                            RelatedEntityId = payment.Order.Id,
                            RelatedEntityType = "Order",
                            ActionUrl = $"/seller/orders/{payment.Order.Id}"
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send ORDER_COMPLETED notification: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine(">>> SETTING ORDER TO CONFIRMED");
                    payment.Order.Status = OrderStatus.CONFIRMED;
                }
            }

            payment.Order.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(long id, long userId, UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.Listing)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            throw new Exception("Order không tồn tại");

        if (order.SellerId != userId && order.BuyerId != userId)
            throw new Exception("Không có quyền cập nhật order này");

        var newStatus = Enum.Parse<OrderStatus>(request.Status, true);

        if (newStatus == OrderStatus.SHIPPING && order.SellerId == userId)
        {
            order.Status = OrderStatus.SHIPPING;
            order.ShippingNote = request.Note;
            
            try
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                {
                    UserId = order.BuyerId,
                    Type = NotificationType.ORDER_SHIPPING,
                    Title = "Đơn hàng đang giao",
                    Message = $"Người bán đã xác nhận gửi hàng cho xe {order.Listing.Title}",
                    RelatedEntityId = order.Id,
                    RelatedEntityType = "Order",
                    ActionUrl = $"/buyer/orders/{order.Id}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send ORDER_SHIPPING notification: {ex.Message}");
            }
        }
        else if (newStatus == OrderStatus.DELIVERED && order.BuyerId == userId)
        {
            order.Status = OrderStatus.DELIVERED;
            order.DeliveredAt = DateTime.UtcNow;
            
            try
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                {
                    UserId = order.SellerId,
                    Type = NotificationType.ORDER_DELIVERED,
                    Title = "Đã giao hàng",
                    Message = $"Người mua đã xác nhận nhận hàng xe {order.Listing.Title}",
                    RelatedEntityId = order.Id,
                    RelatedEntityType = "Order",
                    ActionUrl = $"/seller/orders/{order.Id}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send ORDER_DELIVERED notification: {ex.Message}");
            }
        }
        else if (newStatus == OrderStatus.COMPLETED && order.BuyerId == userId)
        {
            order.Status = OrderStatus.COMPLETED;
            order.CompletedAt = DateTime.UtcNow;
            order.Listing.Status = ListingStatus.SOLD;
            
            try
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                {
                    UserId = order.SellerId,
                    Type = NotificationType.ORDER_COMPLETED,
                    Title = "Đơn hàng hoàn thành",
                    Message = $"Đơn hàng xe {order.Listing.Title} đã hoàn thành",
                    RelatedEntityId = order.Id,
                    RelatedEntityType = "Order",
                    ActionUrl = $"/seller/orders/{order.Id}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send ORDER_COMPLETED notification: {ex.Message}");
            }
        }
        else if (newStatus == OrderStatus.CANCELED)
        {
            order.Status = OrderStatus.CANCELED;
            order.CanceledReason = request.Note;
            
            long notifyUserId = order.BuyerId == userId ? order.SellerId : order.BuyerId;
            try
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                {
                    UserId = notifyUserId,
                    Type = NotificationType.ORDER_CANCELED,
                    Title = "Đơn hàng đã hủy",
                    Message = $"Đơn hàng xe {order.Listing.Title} đã bị hủy. Lý do: {request.Note ?? "Không có lý do"}",
                    RelatedEntityId = order.Id,
                    RelatedEntityType = "Order",
                    ActionUrl = order.BuyerId == userId ? $"/buyer/orders/{order.Id}" : $"/seller/orders/{order.Id}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send ORDER_CANCELED notification: {ex.Message}");
            }
        }
        else
        {
            throw new Exception("Không được phép thực hiện hành động này");
        }

        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(id);
    }
}
