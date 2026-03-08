using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.DTOs.Dispute;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DisputesController : ControllerBase
{
    private readonly IDisputeService _disputeService;

    public DisputesController(IDisputeService disputeService)
    {
        _disputeService = disputeService;
    }

    private long GetUserId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role)!;

    /// <summary>
    /// Create dispute (BUYER/SELLER)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<DisputeDto>>> CreateDispute([FromBody] CreateDisputeRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _disputeService.CreateDisputeAsync(userId, request);
            return Ok(ApiResponse<DisputeDto>.SuccessResponse(result, "Tạo dispute thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DisputeDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get my disputes (current user involved)
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<List<DisputeDto>>>> GetMyDisputes()
    {
        try
        {
            var userId = GetUserId();
            var result = await _disputeService.GetMyDisputesAsync(userId);
            return Ok(ApiResponse<List<DisputeDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<DisputeDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get all disputes (ADMIN/INSPECTOR)
    /// </summary>
    [Authorize(Roles = "ADMIN,INSPECTOR")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DisputeDto>>>> GetAllDisputes([FromQuery] string? status)
    {
        try
        {
            var result = await _disputeService.GetAllDisputesAsync(status);
            return Ok(ApiResponse<List<DisputeDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<DisputeDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get dispute by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DisputeDto>>> GetDispute(long id)
    {
        try
        {
            var result = await _disputeService.GetDisputeByIdAsync(id);
            return Ok(ApiResponse<DisputeDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<DisputeDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Assign dispute to Inspector/Admin (ADMIN only)
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPost("{id}/assign")]
    public async Task<ActionResult<ApiResponse<DisputeDto>>> AssignDispute(long id, [FromBody] AssignDisputeRequest request)
    {
        try
        {
            var adminId = GetUserId();
            var result = await _disputeService.AssignDisputeAsync(id, adminId, request);
            return Ok(ApiResponse<DisputeDto>.SuccessResponse(result, "Assign dispute thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DisputeDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Resolve dispute (ADMIN/INSPECTOR)
    /// </summary>
    [Authorize(Roles = "ADMIN,INSPECTOR")]
    [HttpPost("{id}/resolve")]
    public async Task<ActionResult<ApiResponse<DisputeDto>>> ResolveDispute(long id, [FromBody] ResolveDisputeRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _disputeService.ResolveDisputeAsync(id, userId, request);
            return Ok(ApiResponse<DisputeDto>.SuccessResponse(result, "Giải quyết dispute thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DisputeDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Add comment/event to dispute
    /// </summary>
    [HttpPost("{id}/events")]
    public async Task<ActionResult<ApiResponse<DisputeEventDto>>> AddEvent(long id, [FromBody] AddDisputeEventRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _disputeService.AddEventAsync(id, userId, request);
            return Ok(ApiResponse<DisputeEventDto>.SuccessResponse(result, "Thêm comment thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DisputeEventDto>.ErrorResponse(ex.Message));
        }
    }
}
