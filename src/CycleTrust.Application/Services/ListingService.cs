using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.Listing;

namespace CycleTrust.Application.Services;

public interface IListingService
{
    Task<ListingDto> CreateListingAsync(long sellerId, CreateListingRequest request);
    Task<ListingDto> GetListingByIdAsync(long id);
    Task<List<ListingDto>> GetListingsAsync(string? status = null, long? categoryId = null);
    Task<List<ListingDto>> GetMyListingsAsync(long sellerId);
    Task<ListingDto> UpdateListingAsync(long id, long sellerId, UpdateListingRequest request);
    Task<ListingDto> SubmitForApprovalAsync(long id, long sellerId);
    Task<ListingDto> ApproveListingAsync(long id, long adminId, ApproveListingRequest request);
    Task<InspectionDto> CreateInspectionAsync(long listingId, long inspectorId, CreateInspectionRequest request);
}

public class ListingService : IListingService
{
    private readonly CycleTrustDbContext _context;
    private readonly IMapper _mapper;

    public ListingService(CycleTrustDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ListingDto> CreateListingAsync(long sellerId, CreateListingRequest request)
    {
        var listing = _mapper.Map<Listing>(request);
        listing.SellerId = sellerId;
        
        // Set status based on request, default to DRAFT if not specified
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ListingStatus>(request.Status, true, out var status))
        {
            listing.Status = status;
        }
        else
        {
            listing.Status = ListingStatus.DRAFT;
        }

        _context.Listings.Add(listing);
        await _context.SaveChangesAsync();

        return await GetListingByIdAsync(listing.Id);
    }

    public async Task<ListingDto> GetListingByIdAsync(long id)
    {
        var listing = await _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Brand)
            .Include(l => l.Category)
            .Include(l => l.SizeOption)
            .Include(l => l.Media)
            .Include(l => l.Inspection).ThenInclude(i => i.Inspector)
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

        if (listing == null)
            throw new Exception("Listing không tồn tại");

        return _mapper.Map<ListingDto>(listing);
    }

    public async Task<List<ListingDto>> GetListingsAsync(string? status = null, long? categoryId = null)
    {
        var query = _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Brand)
            .Include(l => l.Category)
            .Include(l => l.SizeOption)
            .Include(l => l.Media)
            .Where(l => !l.IsDeleted);

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<ListingStatus>(status, true);
            query = query.Where(l => l.Status == statusEnum);
        }

        if (categoryId.HasValue)
            query = query.Where(l => l.CategoryId == categoryId);

        var listings = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        return _mapper.Map<List<ListingDto>>(listings);
    }

    public async Task<List<ListingDto>> GetMyListingsAsync(long sellerId)
    {
        var query = _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Brand)
            .Include(l => l.Category)
            .Include(l => l.SizeOption)
            .Include(l => l.Media)
            .Where(l => l.SellerId == sellerId && !l.IsDeleted);

        var listings = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        return _mapper.Map<List<ListingDto>>(listings);
    }

    public async Task<ListingDto> UpdateListingAsync(long id, long sellerId, UpdateListingRequest request)
    {
        var listing = await _context.Listings
            .Include(l => l.Media)
            .FirstOrDefaultAsync(l => l.Id == id && l.SellerId == sellerId && !l.IsDeleted);
            
        if (listing == null)
            throw new Exception("Không tìm thấy listing hoặc không có quyền chỉnh sửa");

        if (listing.Status != ListingStatus.DRAFT && listing.Status != ListingStatus.REJECTED)
            throw new Exception("Chỉ có thể chỉnh sửa listing ở trạng thái DRAFT hoặc REJECTED");

        if (request.Title != null) listing.Title = request.Title;
        if (request.Description != null) listing.Description = request.Description;
        if (request.UsageHistory != null) listing.UsageHistory = request.UsageHistory;
        if (request.LocationText != null) listing.LocationText = request.LocationText;
        if (request.BrandId.HasValue) listing.BrandId = request.BrandId;
        if (request.CategoryId.HasValue) listing.CategoryId = request.CategoryId;
        if (request.SizeOptionId.HasValue) listing.SizeOptionId = request.SizeOptionId;
        if (request.PriceAmount.HasValue) listing.PriceAmount = request.PriceAmount.Value;
        if (request.ConditionNote != null) listing.ConditionNote = request.ConditionNote;
        if (request.YearModel.HasValue) listing.YearModel = request.YearModel;
        
        // Handle media update
        if (request.Media != null)
        {
            // Remove existing media
            var existingMedia = listing.Media.ToList();
            _context.ListingMedia.RemoveRange(existingMedia);
            
            // Add new media
            listing.Media = _mapper.Map<List<ListingMedia>>(request.Media);
        }
        
        // Handle status change: DRAFT -> PENDING_APPROVAL
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ListingStatus>(request.Status, true, out var newStatus))
        {
            if (listing.Status == ListingStatus.DRAFT && newStatus == ListingStatus.PENDING_APPROVAL)
            {
                listing.Status = newStatus;
            }
            else if (listing.Status == ListingStatus.DRAFT && newStatus == ListingStatus.DRAFT)
            {
                // Keep as draft
                listing.Status = ListingStatus.DRAFT;
            }
        }

        listing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetListingByIdAsync(id);
    }

    public async Task<ListingDto> SubmitForApprovalAsync(long id, long sellerId)
    {
        var listing = await _context.Listings.FirstOrDefaultAsync(l => l.Id == id && l.SellerId == sellerId);
        if (listing == null)
            throw new Exception("Không tìm thấy listing");

        if (listing.Status != ListingStatus.DRAFT)
            throw new Exception("Chỉ có thể submit listing ở trạng thái DRAFT");

        listing.Status = ListingStatus.PENDING_APPROVAL;
        listing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetListingByIdAsync(id);
    }

    public async Task<ListingDto> ApproveListingAsync(long id, long adminId, ApproveListingRequest request)
    {
        var listing = await _context.Listings.FirstOrDefaultAsync(l => l.Id == id);
        if (listing == null)
            throw new Exception("Không tìm thấy listing");

        if (request.Approved)
        {
            listing.Status = ListingStatus.APPROVED;
            listing.ApprovedBy = adminId;
            listing.ApprovedAt = DateTime.UtcNow;
        }
        else
        {
            listing.Status = ListingStatus.REJECTED;
            listing.RejectedReason = request.Reason;
        }

        listing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetListingByIdAsync(id);
    }

    public async Task<InspectionDto> CreateInspectionAsync(long listingId, long inspectorId, CreateInspectionRequest request)
    {
        var listing = await _context.Listings.FirstOrDefaultAsync(l => l.Id == listingId);
        if (listing == null)
            throw new Exception("Không tìm thấy listing");

        if (listing.Status != ListingStatus.APPROVED)
            throw new Exception("Chỉ kiểm định được listing đã APPROVED");

        var existingInspection = await _context.Inspections.FirstOrDefaultAsync(i => i.ListingId == listingId);
        if (existingInspection != null)
            throw new Exception("Listing đã được kiểm định");

        var inspection = new Inspection
        {
            ListingId = listingId,
            InspectorId = inspectorId,
            Summary = request.Summary,
            ChecklistJson = request.ChecklistJson,
            ReportUrl = request.ReportUrl
        };

        _context.Inspections.Add(inspection);

        listing.Status = ListingStatus.VERIFIED;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var result = await _context.Inspections
            .Include(i => i.Inspector)
            .FirstAsync(i => i.Id == inspection.Id);

        return _mapper.Map<InspectionDto>(result);
    }
}
