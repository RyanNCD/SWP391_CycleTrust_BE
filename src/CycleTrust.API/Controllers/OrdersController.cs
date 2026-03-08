using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.DTOs.Order;
using CycleTrust.Application.DTOs;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IVNPayService _vnPayService;

    public OrdersController(IOrderService orderService, IVNPayService vnPayService)
    {
        _orderService = orderService;
        _vnPayService = vnPayService;
    }

    private long GetUserId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetMyOrders()
    {
        try
        {
            var userId = GetUserId();
            var role = GetUserRole();
            var result = await _orderService.GetMyOrdersAsync(userId, role);
            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<OrderDto>>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("admin/all")]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetAllOrdersForAdmin(
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            var result = await _orderService.GetAllOrdersForAdminAsync(status, fromDate, toDate);
            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<OrderDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(long id)
    {
        try
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return Ok(ApiResponse<OrderDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<OrderDto>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "BUYER")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _orderService.CreateOrderAsync(userId, request);
            return Ok(ApiResponse<OrderDto>.SuccessResponse(result, "Đặt hàng thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OrderDto>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "BUYER")]
    [HttpPost("payment")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> CreatePayment([FromBody] PaymentRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _orderService.CreatePaymentAsync(userId, request);
            return Ok(ApiResponse<PaymentDto>.SuccessResponse(result, "Tạo payment thành công. URL thanh toán: /mock-payment/" + result.Id));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PaymentDto>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "BUYER")]
    [HttpPost("{id}/payment/deposit")]
    public async Task<ActionResult<ApiResponse<object>>> PayDeposit(long id)
    {
        try
        {
            var userId = GetUserId();
            
            // Get order and validate
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order.BuyerId != userId)
                return Forbid();
            
            if (!order.DepositRequired || order.Status != "DEPOSIT_PENDING")
                return BadRequest(ApiResponse<object>.ErrorResponse("Order không yêu cầu thanh toán cọc hoặc đã thanh toán"));
            
            // Create VNPay payment
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var vnpayRequest = new VNPayPaymentRequestDto
            {
                OrderId = id,
                Amount = order.DepositAmount,
                OrderInfo = $"Thanh toán cọc đơn hàng #{id}",
                ReturnUrl = null, // Use default from config
                PaymentType = PaymentType.DEPOSIT
            };
            
            var vnpayResult = await _vnPayService.CreatePaymentUrl(vnpayRequest, ipAddress);
            
            if (!vnpayResult.Success)
                return BadRequest(ApiResponse<object>.ErrorResponse(vnpayResult.Message ?? "Tạo thanh toán thất bại"));
            
            return Ok(ApiResponse<object>.SuccessResponse(new { paymentUrl = vnpayResult.PaymentUrl }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "BUYER")]
    [HttpPost("{id}/payment/full")]
    public async Task<ActionResult<ApiResponse<object>>> PayFull(long id)
    {
        try
        {
            var userId = GetUserId();
            
            // Get order and validate
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order.BuyerId != userId)
                return Forbid();
            
            long amount = order.PriceAmount;
            string orderInfo = $"Thanh toán toàn bộ đơn hàng #{id}";
            
            // Check if deposit was already paid
            if (order.DepositRequired && order.DepositPaidAt.HasValue)
            {
                amount = order.PriceAmount - order.DepositAmount;
                orderInfo = $"Thanh toán phần còn lại đơn hàng #{id}";
            }
            else if (order.Status != "PLACED")
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Order không ở trạng thái chờ thanh toán"));
            }
            
            // Create VNPay payment
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var vnpayRequest = new VNPayPaymentRequestDto
            {
                OrderId = id,
                Amount = amount,
                OrderInfo = orderInfo,
                ReturnUrl = null, // Use default from config
                PaymentType = PaymentType.FULL
            };
            
            var vnpayResult = await _vnPayService.CreatePaymentUrl(vnpayRequest, ipAddress);
            
            if (!vnpayResult.Success)
                return BadRequest(ApiResponse<object>.ErrorResponse(vnpayResult.Message ?? "Tạo thanh toán thất bại"));
            
            return Ok(ApiResponse<object>.SuccessResponse(new { paymentUrl = vnpayResult.PaymentUrl }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("payment/callback")]
    public async Task<ActionResult<ApiResponse<PaymentDto>>> PaymentCallback([FromBody] PaymentCallbackRequest request)
    {
        try
        {
            var result = await _orderService.ProcessPaymentCallbackAsync(request);
            return Ok(ApiResponse<PaymentDto>.SuccessResponse(result, "Thanh toán thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PaymentDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateOrderStatus(long id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _orderService.UpdateOrderStatusAsync(id, userId, request);
            return Ok(ApiResponse<OrderDto>.SuccessResponse(result, "Cập nhật trạng thái thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OrderDto>.ErrorResponse(ex.Message));
        }
    }
}
