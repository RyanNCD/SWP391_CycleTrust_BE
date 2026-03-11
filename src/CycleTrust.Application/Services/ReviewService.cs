using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.Review;

namespace CycleTrust.Application.Services;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(long buyerId, CreateReviewRequest request);
    Task<List<ReviewDto>> GetReviewsBySellerIdAsync(long sellerId);
    Task<SellerRatingDto> GetSellerRatingAsync(long sellerId);
}

public class ReviewService : IReviewService
{
    private readonly CycleTrustDbContext _context;
    private readonly IMapper _mapper;

    public ReviewService(CycleTrustDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ReviewDto> CreateReviewAsync(long buyerId, CreateReviewRequest request)
    {
        // Validate order exists and belongs to buyer
        var order = await _context.Orders
            .Include(o => o.Seller)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId);

        if (order == null)
            throw new Exception("Order không tồn tại");

        if (order.BuyerId != buyerId)
            throw new Exception("Bạn không có quyền review order này");

        // Check order is completed
        if (order.Status != OrderStatus.COMPLETED)
            throw new Exception("Chỉ có thể review sau khi order hoàn thành");

        // Check if already reviewed
        var existingReview = await _context.Reviews
            .AnyAsync(r => r.OrderId == request.OrderId);

        if (existingReview)
            throw new Exception("Order này đã được review");

        // Validate rating
        if (request.Rating < 1 || request.Rating > 5)
            throw new Exception("Rating phải từ 1 đến 5");

        var review = new Review
        {
            OrderId = order.Id,
            BuyerId = buyerId,
            SellerId = order.SellerId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _context.Reviews.Add(review);

        // Update seller rating
        var seller = await _context.Users.FindAsync(order.SellerId);
        if (seller != null)
        {
            var allSellerReviews = await _context.Reviews
                .Where(r => r.SellerId == order.SellerId)
                .ToListAsync();
            
            // Include the new review in calculation
            allSellerReviews.Add(review);
            
            seller.RatingCount = allSellerReviews.Count;
            seller.RatingAvg = (decimal)allSellerReviews.Average(r => r.Rating);
            seller.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var reviewDto = _mapper.Map<ReviewDto>(review);
        reviewDto.SellerName = order.Seller.FullName;
        return reviewDto;
    }

    public async Task<List<ReviewDto>> GetReviewsBySellerIdAsync(long sellerId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.Buyer)
            .Include(r => r.Seller)
            .Where(r => r.SellerId == sellerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var reviewDtos = _mapper.Map<List<ReviewDto>>(reviews);
        
        return reviewDtos;
    }

    public async Task<SellerRatingDto> GetSellerRatingAsync(long sellerId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.SellerId == sellerId)
            .ToListAsync();

        if (!reviews.Any())
        {
            return new SellerRatingDto
            {
                SellerId = sellerId,
                AverageRating = 0,
                TotalReviews = 0
            };
        }

        var average = reviews.Average(r => r.Rating);

        return new SellerRatingDto
        {
            SellerId = sellerId,
            AverageRating = Math.Round(average, 1),
            TotalReviews = reviews.Count
        };
    }
}
