using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.Order;

namespace CycleTrust.Application.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(long buyerId, CreateOrderRequest request);
    Task<OrderDto> GetOrderByIdAsync(long id);
    Task<List<OrderDto>> GetMyOrdersAsync(long userId, string role);
    Task<List<OrderDto>> GetAllOrdersForAdminAsync(string? status, DateTime? fromDate, DateTime? toDate);
    Task<PaymentDto> CreatePaymentAsync(long userId, PaymentRequest request);
    Task<PaymentDto> ProcessPaymentCallbackAsync(PaymentCallbackRequest request);
    Task<OrderDto> UpdateOrderStatusAsync(long id, long userId, UpdateOrderStatusRequest request);
}

public class OrderService : IOrderService
{
    private readonly CycleTrustDbContext _context;
    private readonly IMapper _mapper;

    public OrderService(CycleTrustDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

        // Filter by status
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
        {
            query = query.Where(o => o.Status == orderStatus);
        }

        // Filter by date range
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

    public async Task<PaymentDto> ProcessPaymentCallbackAsync(PaymentCallbackRequest request)
    {
        var payment = await _context.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o.Listing)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId);

        if (payment == null)
            throw new Exception("Payment không tồn tại");

        var status = Enum.Parse<PaymentStatus>(request.Status, true);
        payment.Status = status;
        payment.ProviderTxnId = request.ProviderTxnId;

        if (status == PaymentStatus.PAID)
        {
            payment.PaidAt = DateTime.UtcNow;

            if (payment.Type == PaymentType.DEPOSIT)
            {
                payment.Order.Status = OrderStatus.DEPOSIT_PAID;
                payment.Order.DepositPaidAt = DateTime.UtcNow;
                payment.Order.ReserveExpiresAt = DateTime.UtcNow.AddDays(7);
            }
            else if (payment.Type == PaymentType.FULL)
            {
                // If this is remaining payment after delivery, complete the order
                if (payment.Order.Status == OrderStatus.DELIVERED && payment.Order.DepositPaidAt.HasValue)
                {
                    payment.Order.Status = OrderStatus.COMPLETED;
                    payment.Order.CompletedAt = DateTime.UtcNow;
                    payment.Order.Listing.Status = ListingStatus.SOLD;
                }
                else
                {
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
        }
        else if (newStatus == OrderStatus.DELIVERED && order.BuyerId == userId)
        {
            order.Status = OrderStatus.DELIVERED;
            order.DeliveredAt = DateTime.UtcNow;
        }
        else if (newStatus == OrderStatus.COMPLETED && order.BuyerId == userId)
        {
            order.Status = OrderStatus.COMPLETED;
            order.CompletedAt = DateTime.UtcNow;
            order.Listing.Status = ListingStatus.SOLD;
        }
        else if (newStatus == OrderStatus.CANCELED)
        {
            order.Status = OrderStatus.CANCELED;
            order.CanceledReason = request.Note;
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
