using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.DTOs.Order;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
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
