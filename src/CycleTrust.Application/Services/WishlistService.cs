using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.Wishlist;

namespace CycleTrust.Application.Services;

public interface IWishlistService
{
    Task<bool> AddToWishlistAsync(long buyerId, long listingId);
    Task<bool> RemoveFromWishlistAsync(long buyerId, long listingId);
    Task<List<WishlistItemDto>> GetMyWishlistAsync(long buyerId);
    Task<bool> IsInWishlistAsync(long buyerId, long listingId);
}

public class WishlistService : IWishlistService
{
    private readonly CycleTrustDbContext _context;
    private readonly IMapper _mapper;

    public WishlistService(CycleTrustDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<bool> AddToWishlistAsync(long buyerId, long listingId)
    {
        // Check if listing exists
        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId);

        if (listing == null)
            throw new Exception("Listing không tồn tại");

        // Check if already in wishlist
        var existing = await _context.Wishlists
            .AnyAsync(w => w.BuyerId == buyerId && w.ListingId == listingId);

        if (existing)
            throw new Exception("Listing đã có trong danh sách yêu thích");

        var wishlist = new Wishlist
        {
            BuyerId = buyerId,
            ListingId = listingId
        };

        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveFromWishlistAsync(long buyerId, long listingId)
    {
        var wishlist = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.BuyerId == buyerId && w.ListingId == listingId);

        if (wishlist == null)
            throw new Exception("Listing không có trong danh sách yêu thích");

        _context.Wishlists.Remove(wishlist);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<WishlistItemDto>> GetMyWishlistAsync(long buyerId)
    {
        var wishlists = await _context.Wishlists
            .Include(w => w.Listing)
            .ThenInclude(l => l.Media)
            .Where(w => w.BuyerId == buyerId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        var items = wishlists.Select(w => new WishlistItemDto
        {
            ListingId = w.ListingId,
            Title = w.Listing.Title,
            PriceAmount = w.Listing.PriceAmount,
            Currency = w.Listing.Currency,
            MainImageUrl = w.Listing.Media
                .Where(m => m.Type == MediaType.IMAGE)
                .OrderBy(m => m.SortOrder)
                .FirstOrDefault()?.Url,
            Status = w.Listing.Status.ToString(),
            AddedAt = w.CreatedAt
        }).ToList();

        return items;
    }

    public async Task<bool> IsInWishlistAsync(long buyerId, long listingId)
    {
        return await _context.Wishlists
            .AnyAsync(w => w.BuyerId == buyerId && w.ListingId == listingId);
    }
}
