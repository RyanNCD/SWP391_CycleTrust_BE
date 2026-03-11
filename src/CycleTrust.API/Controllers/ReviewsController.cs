using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.DTOs.Review;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private long GetUserId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "BUYER")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> CreateReview([FromBody] CreateReviewRequest request)
    {
        try
        {
            var buyerId = GetUserId();
            var result = await _reviewService.CreateReviewAsync(buyerId, request);
            return Ok(ApiResponse<ReviewDto>.SuccessResponse(result, "Đánh giá thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ReviewDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("seller/{sellerId}")]
    public async Task<ActionResult<ApiResponse<List<ReviewDto>>>> GetSellerReviews(long sellerId)
    {
        try
        {
            var result = await _reviewService.GetReviewsBySellerIdAsync(sellerId);
            return Ok(ApiResponse<List<ReviewDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<ReviewDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("seller/{sellerId}/rating")]
    public async Task<ActionResult<ApiResponse<SellerRatingDto>>> GetSellerRating(long sellerId)
    {
        try
        {
            var result = await _reviewService.GetSellerRatingAsync(sellerId);
            return Ok(ApiResponse<SellerRatingDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SellerRatingDto>.ErrorResponse(ex.Message));
        }
    }
}
