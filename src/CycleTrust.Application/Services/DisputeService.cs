using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.Dispute;

namespace CycleTrust.Application.Services;

public interface IDisputeService
{
    Task<DisputeDto> CreateDisputeAsync(long userId, CreateDisputeRequest request);
    Task<List<DisputeDto>> GetMyDisputesAsync(long userId);
    Task<List<DisputeDto>> GetAllDisputesAsync(string? status);
    Task<DisputeDto> GetDisputeByIdAsync(long id);
    Task<DisputeDto> AssignDisputeAsync(long disputeId, long adminId, AssignDisputeRequest request);
    Task<DisputeDto> ResolveDisputeAsync(long disputeId, long userId, ResolveDisputeRequest request);
    Task<DisputeEventDto> AddEventAsync(long disputeId, long userId, AddDisputeEventRequest request);
}

public class DisputeService : IDisputeService
{
    private readonly CycleTrustDbContext _context;
    private readonly IMapper _mapper;

    public DisputeService(CycleTrustDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DisputeDto> CreateDisputeAsync(long userId, CreateDisputeRequest request)
    {
        // Validate order exists
        var order = await _context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Seller)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId);

        if (order == null)
            throw new Exception("Order không tồn tại");

        // Check if user is involved in the order
        if (order.BuyerId != userId && order.SellerId != userId)
            throw new Exception("Bạn không có quyền tạo dispute cho order này");

        // Check if order can be disputed
        if (order.Status != OrderStatus.CONFIRMED && 
            order.Status != OrderStatus.SHIPPING && 
            order.Status != OrderStatus.DELIVERED)
            throw new Exception("Không thể tạo dispute cho order ở trạng thái này");

        // Check if dispute already exists
        var existingDispute = await _context.Disputes
            .AnyAsync(d => d.OrderId == request.OrderId);

        if (existingDispute)
            throw new Exception("Dispute đã tồn tại cho order này");

        var dispute = new Dispute
        {
            OrderId = order.Id,
            OpenedBy = userId,
            Status = DisputeStatus.OPEN,
            Summary = request.Summary
        };

        _context.Disputes.Add(dispute);
        
        // Update order status
        order.Status = OrderStatus.DISPUTED;
        
        await _context.SaveChangesAsync();

        // Add initial event
        var initialEvent = new DisputeEvent
        {
            DisputeId = dispute.Id,
            ActorId = userId,
            Message = $"Dispute được tạo: {request.Summary}"
        };
        _context.DisputeEvents.Add(initialEvent);
        await _context.SaveChangesAsync();

        // Reload with includes
        dispute = await _context.Disputes
            .Include(d => d.OpenedByUser)
            .Include(d => d.Events)
            .ThenInclude(e => e.Actor)
            .FirstAsync(d => d.Id == dispute.Id);

        return MapToDto(dispute);
    }

    public async Task<List<DisputeDto>> GetMyDisputesAsync(long userId)
    {
        var disputes = await _context.Disputes
            .Include(d => d.OpenedByUser)
            .Include(d => d.AssignedInspector)
            .Include(d => d.AssignedAdmin)
            .Include(d => d.Order)
            .Include(d => d.Events)
            .ThenInclude(e => e.Actor)
            .Where(d => d.OpenedBy == userId || 
                       d.Order.BuyerId == userId || 
                       d.Order.SellerId == userId ||
                       d.AssignedInspectorId == userId ||
                       d.AssignedAdminId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return disputes.Select(MapToDto).ToList();
    }

    public async Task<List<DisputeDto>> GetAllDisputesAsync(string? status)
    {
        var query = _context.Disputes
            .Include(d => d.OpenedByUser)
            .Include(d => d.AssignedInspector)
            .Include(d => d.AssignedAdmin)
            .Include(d => d.Events)
            .ThenInclude(e => e.Actor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<DisputeStatus>(status, true, out var disputeStatus))
        {
            query = query.Where(d => d.Status == disputeStatus);
        }

        var disputes = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return disputes.Select(MapToDto).ToList();
    }

    public async Task<DisputeDto> GetDisputeByIdAsync(long id)
    {
        var dispute = await _context.Disputes
            .Include(d => d.OpenedByUser)
            .Include(d => d.AssignedInspector)
            .Include(d => d.AssignedAdmin)
            .Include(d => d.Events)
            .ThenInclude(e => e.Actor)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dispute == null)
            throw new Exception("Dispute không tồn tại");

        return MapToDto(dispute);
    }

    public async Task<DisputeDto> AssignDisputeAsync(long disputeId, long adminId, AssignDisputeRequest request)
    {
        var dispute = await _context.Disputes
            .Include(d => d.OpenedByUser)
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
            throw new Exception("Dispute không tồn tại");

        if (request.InspectorId.HasValue)
        {
            var inspector = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.InspectorId.Value && u.Role == UserRole.INSPECTOR);
            
            if (inspector == null)
                throw new Exception("Inspector không tồn tại");

            dispute.AssignedInspectorId = request.InspectorId.Value;
        }

        if (request.AdminId.HasValue)
        {
            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.AdminId.Value && u.Role == UserRole.ADMIN);
            
            if (admin == null)
                throw new Exception("Admin không tồn tại");

            dispute.AssignedAdminId = request.AdminId.Value;
        }

        dispute.Status = DisputeStatus.ASSIGNED;
        dispute.UpdatedAt = DateTime.UtcNow;

        // Add event
        var assignEvent = new DisputeEvent
        {
            DisputeId = dispute.Id,
            ActorId = adminId,
            Message = $"Dispute đã được assign"
        };
        _context.DisputeEvents.Add(assignEvent);

        await _context.SaveChangesAsync();

        // Reload
        dispute = await _context.Disputes
            .Include(d => d.OpenedByUser)
            .Include(d => d.AssignedInspector)
            .Include(d => d.AssignedAdmin)
            .Include(d => d.Events)
            .ThenInclude(e => e.Actor)
            .FirstAsync(d => d.Id == dispute.Id);

        return MapToDto(dispute);
    }

    public async Task<DisputeDto> ResolveDisputeAsync(long disputeId, long userId, ResolveDisputeRequest request)
    {
        var dispute = await _context.Disputes
            .Include(d => d.Order)
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
            throw new Exception("Dispute không tồn tại");

        // Check authorization
        var user = await _context.Users.FindAsync(userId);
        if (user == null || (user.Role != UserRole.ADMIN && user.Role != UserRole.INSPECTOR))
            throw new Exception("Bạn không có quyền resolve dispute");

        if (user.Role == UserRole.INSPECTOR && dispute.AssignedInspectorId != userId)
            throw new Exception("Dispute không được assign cho bạn");

        dispute.Status = DisputeStatus.RESOLVED;
        dispute.Resolution = request.Resolution;
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.UpdatedAt = DateTime.UtcNow;

        // Add resolution event
        var resolveEvent = new DisputeEvent
        {
            DisputeId = dispute.Id,
            ActorId = userId,
            Message = $"Dispute đã được giải quyết: {request.Resolution}"
        };
        _context.DisputeEvents.Add(resolveEvent);

        // Update order status back to previous state (CONFIRMED)
        dispute.Order.Status = OrderStatus.CONFIRMED;

        await _context.SaveChangesAsync();

        // Reload
        dispute = await _context.Disputes
            .Include(d => d.OpenedByUser)
            .Include(d => d.AssignedInspector)
            .Include(d => d.AssignedAdmin)
            .Include(d => d.Events)
            .ThenInclude(e => e.Actor)
            .FirstAsync(d => d.Id == dispute.Id);

        return MapToDto(dispute);
    }

    public async Task<DisputeEventDto> AddEventAsync(long disputeId, long userId, AddDisputeEventRequest request)
    {
        var dispute = await _context.Disputes
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
            throw new Exception("Dispute không tồn tại");

        var disputeEvent = new DisputeEvent
        {
            DisputeId = disputeId,
            ActorId = userId,
            Message = request.Message
        };

        _context.DisputeEvents.Add(disputeEvent);
        dispute.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reload with actor
        disputeEvent = await _context.DisputeEvents
            .Include(e => e.Actor)
            .FirstAsync(e => e.Id == disputeEvent.Id);

        return new DisputeEventDto
        {
            Id = disputeEvent.Id,
            ActorId = disputeEvent.ActorId,
            ActorName = disputeEvent.Actor?.FullName,
            Message = disputeEvent.Message,
            CreatedAt = disputeEvent.CreatedAt
        };
    }

    private DisputeDto MapToDto(Dispute dispute)
    {
        return new DisputeDto
        {
            Id = dispute.Id,
            OrderId = dispute.OrderId,
            OpenedBy = dispute.OpenedBy,
            OpenedByName = dispute.OpenedByUser.FullName,
            Status = dispute.Status.ToString(),
            AssignedInspectorId = dispute.AssignedInspectorId,
            AssignedInspectorName = dispute.AssignedInspector?.FullName,
            AssignedAdminId = dispute.AssignedAdminId,
            AssignedAdminName = dispute.AssignedAdmin?.FullName,
            Summary = dispute.Summary,
            Resolution = dispute.Resolution,
            ResolvedAt = dispute.ResolvedAt,
            CreatedAt = dispute.CreatedAt,
            UpdatedAt = dispute.UpdatedAt,
            Events = dispute.Events.Select(e => new DisputeEventDto
            {
                Id = e.Id,
                ActorId = e.ActorId,
                ActorName = e.Actor?.FullName,
                Message = e.Message,
                CreatedAt = e.CreatedAt
            }).OrderBy(e => e.CreatedAt).ToList()
        };
    }
}
