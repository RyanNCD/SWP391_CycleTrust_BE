using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.DTOs.Wishlist;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "BUYER")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    private long GetUserId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WishlistItemDto>>>> GetMyWishlist()
    {
        try
        {
            var buyerId = GetUserId();
            var result = await _wishlistService.GetMyWishlistAsync(buyerId);
            return Ok(ApiResponse<List<WishlistItemDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<WishlistItemDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{listingId}")]
    public async Task<ActionResult<ApiResponse<bool>>> AddToWishlist(long listingId)
    {
        try
        {
            var buyerId = GetUserId();
            var result = await _wishlistService.AddToWishlistAsync(buyerId, listingId);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Đã thêm vào danh sách yêu thích"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{listingId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFromWishlist(long listingId)
    {
        try
        {
            var buyerId = GetUserId();
            var result = await _wishlistService.RemoveFromWishlistAsync(buyerId, listingId);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Đã xóa khỏi danh sách yêu thích"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("check/{listingId}")]
    public async Task<ActionResult<ApiResponse<bool>>> IsInWishlist(long listingId)
    {
        try
        {
            var buyerId = GetUserId();
            var result = await _wishlistService.IsInWishlistAsync(buyerId, listingId);
            return Ok(ApiResponse<bool>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }
}
