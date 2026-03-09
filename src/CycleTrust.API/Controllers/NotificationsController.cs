using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.DTOs.Notification;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private long GetUserId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetNotifications([FromQuery] int limit = 20)
    {
        try
        {
            var userId = GetUserId();
            var result = await _notificationService.GetUserNotificationsAsync(userId, limit);
            return Ok(ApiResponse<List<NotificationDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<NotificationDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<NotificationSummaryDto>>> GetNotificationSummary()
    {
        try
        {
            var userId = GetUserId();
            var result = await _notificationService.GetNotificationSummaryAsync(userId);
            return Ok(ApiResponse<NotificationSummaryDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<NotificationSummaryDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> MarkAsRead(long id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _notificationService.MarkAsReadAsync(id, userId);
            return Ok(ApiResponse<NotificationDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<NotificationDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead()
    {
        try
        {
            var userId = GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Đã đánh dấu đọc tất cả thông báo" }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteNotification(long id)
    {
        try
        {
            var userId = GetUserId();
            await _notificationService.DeleteNotificationAsync(id, userId);
            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Đã xóa thông báo" }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
