using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.DTOs.Listing;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;

    public ListingsController(IListingService listingService)
    {
        _listingService = listingService;
    }

    private long GetUserId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ListingDto>>>> GetListings([FromQuery] string? status, [FromQuery] long? categoryId)
    {
        try
        {
            var result = await _listingService.GetListingsAsync(status, categoryId);
            return Ok(ApiResponse<List<ListingDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<ListingDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ListingDto>>> GetListing(long id)
    {
        try
        {
            var result = await _listingService.GetListingByIdAsync(id);
            return Ok(ApiResponse<ListingDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse<ListingDto>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "SELLER")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ListingDto>>> CreateListing([FromBody] CreateListingRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _listingService.CreateListingAsync(userId, request);
            return Ok(ApiResponse<ListingDto>.SuccessResponse(result, "Tạo listing thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ListingDto>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "SELLER")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ListingDto>>> UpdateListing(long id, [FromBody] UpdateListingRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _listingService.UpdateListingAsync(id, userId, request);
            return Ok(ApiResponse<ListingDto>.SuccessResponse(result, "Cập nhật thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ListingDto>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "SELLER")]
    [HttpPost("{id}/submit")]
    public async Task<ActionResult<ApiResponse<ListingDto>>> SubmitForApproval(long id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _listingService.SubmitForApprovalAsync(id, userId);
            return Ok(ApiResponse<ListingDto>.SuccessResponse(result, "Đã gửi yêu cầu duyệt"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ListingDto>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ApiResponse<ListingDto>>> ApproveListing(long id, [FromBody] ApproveListingRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _listingService.ApproveListingAsync(id, userId, request);
            return Ok(ApiResponse<ListingDto>.SuccessResponse(result, request.Approved ? "Đã duyệt" : "Đã từ chối"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ListingDto>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "INSPECTOR")]
    [HttpPost("{id}/inspection")]
    public async Task<ActionResult<ApiResponse<InspectionDto>>> CreateInspection(long id, [FromBody] CreateInspectionRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _listingService.CreateInspectionAsync(id, userId, request);
            return Ok(ApiResponse<InspectionDto>.SuccessResponse(result, "Tạo báo cáo kiểm định thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<InspectionDto>.ErrorResponse(ex.Message));
        }
    }
}
