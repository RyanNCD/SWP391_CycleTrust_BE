using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.Listing;
using CycleTrust.Application.DTOs.Notification;
using CycleTrust.Application.DTOs.Common;

namespace CycleTrust.Application.Services;

public interface IListingService
{
    Task<ListingDto> CreateListingAsync(long sellerId, CreateListingRequest request);
    Task<ListingDto> GetListingByIdAsync(long id);
    Task<List<ListingDto>> GetListingsAsync(string? status = null, long? categoryId = null);
    Task<List<ListingDto>> GetAllListingsForAdminAsync(string? status = null);
    Task<List<ListingDto>> GetMyInspectionsAsync(long inspectorId);
    Task<PagedResponse<ListingDto>> GetListingsPagedAsync(int pageNumber, int pageSize, string? status = null, long? categoryId = null, long? brandId = null, decimal? minPrice = null, decimal? maxPrice = null, string? search = null);
    Task<List<ListingDto>> GetMyListingsAsync(long sellerId);
    Task<PagedResponse<ListingDto>> GetMyListingsPagedAsync(long sellerId, int pageNumber, int pageSize);
    Task<ListingDto> UpdateListingAsync(long id, long sellerId, UpdateListingRequest request);
    Task<ListingDto> SubmitForApprovalAsync(long id, long sellerId);
    Task<ListingDto> ApproveListingAsync(long id, long adminId, ApproveListingRequest request);
    Task<InspectionDto> CreateInspectionAsync(long listingId, long inspectorId, CreateInspectionRequest request);
    Task<InspectionDto> UpdateInspectionAsync(long listingId, long userId, string userRole, CreateInspectionRequest request);
}

public class ListingService : IListingService
{
    private readonly CycleTrustDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public ListingService(CycleTrustDbContext context, IMapper mapper, INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<ListingDto> CreateListingAsync(long sellerId, CreateListingRequest request)
    {
        var listing = _mapper.Map<Listing>(request);
        listing.SellerId = sellerId;
        
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

        query = query.Where(l => !_context.Orders.Any(o => 
            o.ListingId == l.Id && 
            o.Status != OrderStatus.CANCELED && 
            o.Status != OrderStatus.COMPLETED));

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

    public async Task<List<ListingDto>> GetAllListingsForAdminAsync(string? status = null)
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

        var listings = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        return _mapper.Map<List<ListingDto>>(listings);
    }

    public async Task<List<ListingDto>> GetMyInspectionsAsync(long inspectorId)
    {
        var listings = await _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Brand)
            .Include(l => l.Category)
            .Include(l => l.SizeOption)
            .Include(l => l.Media)
            .Include(l => l.Inspection)
                .ThenInclude(i => i.Inspector)
            .Where(l => !l.IsDeleted && l.Inspection != null && l.Inspection.InspectorId == inspectorId)
            .OrderByDescending(l => l.Inspection!.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ListingDto>>(listings);
    }

    public async Task<PagedResponse<ListingDto>> GetListingsPagedAsync(
        int pageNumber, 
        int pageSize, 
        string? status = null, 
        long? categoryId = null, 
        long? brandId = null, 
        decimal? minPrice = null, 
        decimal? maxPrice = null, 
        string? search = null)
    {
        var query = _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Brand)
            .Include(l => l.Category)
            .Include(l => l.SizeOption)
            .Include(l => l.Media)
            .Where(l => !l.IsDeleted);

        query = query.Where(l => !_context.Orders.Any(o => 
            o.ListingId == l.Id && 
            o.Status != OrderStatus.CANCELED && 
            o.Status != OrderStatus.COMPLETED));

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<ListingStatus>(status, true);
            query = query.Where(l => l.Status == statusEnum);
        }

        if (categoryId.HasValue)
            query = query.Where(l => l.CategoryId == categoryId);

        if (brandId.HasValue)
            query = query.Where(l => l.BrandId == brandId);

        if (minPrice.HasValue)
            query = query.Where(l => l.PriceAmount >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(l => l.PriceAmount <= maxPrice.Value);

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(l => 
                l.Title.ToLower().Contains(searchLower) ||
                (l.Description != null && l.Description.ToLower().Contains(searchLower)) ||
                (l.Brand != null && l.Brand.Name.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync();

        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ListingDto>
        {
            Items = _mapper.Map<List<ListingDto>>(listings),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
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

    public async Task<PagedResponse<ListingDto>> GetMyListingsPagedAsync(long sellerId, int pageNumber, int pageSize)
    {
        var query = _context.Listings
            .Include(l => l.Seller)
            .Include(l => l.Brand)
            .Include(l => l.Category)
            .Include(l => l.SizeOption)
            .Include(l => l.Media)
            .Where(l => l.SellerId == sellerId && !l.IsDeleted);

        var totalCount = await query.CountAsync();

        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ListingDto>
        {
            Items = _mapper.Map<List<ListingDto>>(listings),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
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
        
        if (request.Media != null)
        {
            var existingMedia = listing.Media.ToList();
            _context.ListingMedia.RemoveRange(existingMedia);
            
            listing.Media = _mapper.Map<List<ListingMedia>>(request.Media);
        }
        
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ListingStatus>(request.Status, true, out var newStatus))
        {
            if (listing.Status == ListingStatus.DRAFT && newStatus == ListingStatus.PENDING_APPROVAL)
            {
                listing.Status = newStatus;
            }
            else if (listing.Status == ListingStatus.DRAFT && newStatus == ListingStatus.DRAFT)
            {
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
            
            try
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                {
                    UserId = listing.SellerId,
                    Type = NotificationType.LISTING_APPROVED,
                    Title = "Listing đã được duyệt",
                    Message = $"Xe {listing.Title} đã được admin phê duyệt và có thể kiểm định",
                    RelatedEntityId = id,
                    RelatedEntityType = "Listing",
                    ActionUrl = $"/seller/listings/{id}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send LISTING_APPROVED notification: {ex.Message}");
            }
        }
        else
        {
            listing.Status = ListingStatus.REJECTED;
            listing.RejectedReason = request.Reason;
            
            try
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                {
                    UserId = listing.SellerId,
                    Type = NotificationType.LISTING_REJECTED,
                    Title = "Listing bị từ chối",
                    Message = $"Xe {listing.Title} đã bị từ chối. Lý do: {request.Reason ?? "Không có lý do"}",
                    RelatedEntityId = id,
                    RelatedEntityType = "Listing",
                    ActionUrl = $"/seller/listings/{id}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send LISTING_REJECTED notification: {ex.Message}");
            }
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

        try
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                UserId = listing.SellerId,
                Type = NotificationType.INSPECTION_COMPLETED,
                Title = "Kiểm định hoàn tất",
                Message = $"Xe {listing.Title} đã được kiểm định và chuyển sang trạng thái VERIFIED",
                RelatedEntityId = listingId,
                RelatedEntityType = "Listing",
                ActionUrl = $"/seller/listings/{listingId}"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send INSPECTION_COMPLETED notification: {ex.Message}");
        }

        var result = await _context.Inspections
            .Include(i => i.Inspector)
            .FirstAsync(i => i.Id == inspection.Id);

        return _mapper.Map<InspectionDto>(result);
    }

    public async Task<InspectionDto> UpdateInspectionAsync(long listingId, long userId, string userRole, CreateInspectionRequest request)
    {
        var listing = await _context.Listings
            .Include(l => l.Inspection)
            .FirstOrDefaultAsync(l => l.Id == listingId);
            
        if (listing == null)
            throw new Exception("Không tìm thấy listing");

        if (listing.Inspection == null)
            throw new Exception("Listing chưa được kiểm định");

        if (userRole != "ADMIN" && listing.Inspection.InspectorId != userId)
            throw new Exception("Bạn không có quyền chỉnh sửa báo cáo kiểm định này");

        listing.Inspection.Summary = request.Summary;
        listing.Inspection.ChecklistJson = request.ChecklistJson;
        listing.Inspection.ReportUrl = request.ReportUrl;
        listing.Inspection.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        try
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                UserId = listing.SellerId,
                Type = NotificationType.INSPECTION_COMPLETED,
                Title = "Báo cáo kiểm định đã cập nhật",
                Message = $"Báo cáo kiểm định cho xe {listing.Title} đã được cập nhật",
                RelatedEntityId = listingId,
                RelatedEntityType = "Listing",
                ActionUrl = $"/seller/listings/{listingId}"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send INSPECTION_COMPLETED notification: {ex.Message}");
        }

        var result = await _context.Inspections
            .Include(i => i.Inspector)
            .FirstAsync(i => i.Id == listing.Inspection.Id);

        return _mapper.Map<InspectionDto>(result);
    }
}
