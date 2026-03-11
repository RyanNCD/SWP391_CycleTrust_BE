using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.User;

namespace CycleTrust.Application.Services;

public interface IUserManagementService
{
    Task<UserListResponse> GetAllUsersAsync(UserFilterRequest filter);
    Task<UserDto> GetUserByIdAsync(long id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(long id, UpdateUserRequest request);
    Task DeleteUserAsync(long id);
    Task<List<UserDto>> GetPendingSellersAsync();
    Task<UserDto> ApproveSellerAsync(long id);
    Task<UserDto> RejectSellerAsync(long id);
    Task<UserDto> ToggleActiveAsync(long id);
}

public class UserManagementService : IUserManagementService
{
    private readonly CycleTrustDbContext _context;

    public UserManagementService(CycleTrustDbContext context)
    {
        _context = context;
    }

    public async Task<UserListResponse> GetAllUsersAsync(UserFilterRequest filter)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Role))
        {
            if (Enum.TryParse<UserRole>(filter.Role, true, out var role))
            {
                query = query.Where(u => u.Role == role);
            }
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        if (!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(u =>
                u.FullName.Contains(filter.Search) ||
                (u.Email != null && u.Email.Contains(filter.Search)) ||
                (u.Phone != null && u.Phone.Contains(filter.Search))
            );
        }

        var total = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((filter.Page - 1) * filter.Limit)
            .Take(filter.Limit)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role.ToString(),
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                IsActive = u.IsActive,
                ApprovalStatus = u.ApprovalStatus.HasValue ? u.ApprovalStatus.Value.ToString() : null,
                RatingAvg = u.RatingAvg,
                RatingCount = u.RatingCount,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();

        return new UserListResponse
        {
            Users = users,
            Total = total,
            Page = filter.Page,
            Limit = filter.Limit
        };
    }

    public async Task<UserDto> GetUserByIdAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new Exception("Không tìm thấy user");

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive,
            ApprovalStatus = user.ApprovalStatus.HasValue ? user.ApprovalStatus.Value.ToString() : null,
            RatingAvg = user.RatingAvg,
            RatingCount = user.RatingCount,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        if (!string.IsNullOrEmpty(request.Email))
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new Exception("Email đã tồn tại");
        }

        if (!string.IsNullOrEmpty(request.Phone))
        {
            if (await _context.Users.AnyAsync(u => u.Phone == request.Phone))
                throw new Exception("Số điện thoại đã tồn tại");
        }

        var role = Enum.Parse<UserRole>(request.Role, true);

        var user = new User
        {
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = role,
            IsActive = true,
            ApprovalStatus = role == UserRole.SELLER ? Core.Enums.ApprovalStatus.APPROVED : null
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(user.Id);
    }

    public async Task<UserDto> UpdateUserAsync(long id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new Exception("Không tìm thấy user");

        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
                throw new Exception("Email đã tồn tại");
            user.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.Phone) && request.Phone != user.Phone)
        {
            if (await _context.Users.AnyAsync(u => u.Phone == request.Phone && u.Id != id))
                throw new Exception("Số điện thoại đã tồn tại");
            user.Phone = request.Phone;
        }

        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.Role))
            user.Role = Enum.Parse<UserRole>(request.Role, true);

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(user.Id);
    }

    public async Task DeleteUserAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new Exception("Không tìm thấy user");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<List<UserDto>> GetPendingSellersAsync()
    {
        var users = await _context.Users
            .Where(u => u.Role == UserRole.SELLER && u.ApprovalStatus == Core.Enums.ApprovalStatus.PENDING)
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role.ToString(),
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                IsActive = u.IsActive,
                ApprovalStatus = u.ApprovalStatus.HasValue ? u.ApprovalStatus.Value.ToString() : null,
                RatingAvg = u.RatingAvg,
                RatingCount = u.RatingCount,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();

        return users;
    }

    public async Task<UserDto> ApproveSellerAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new Exception("Không tìm thấy user");

        if (user.Role != UserRole.SELLER)
            throw new Exception("User không phải là seller");

        user.ApprovalStatus = Core.Enums.ApprovalStatus.APPROVED;
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(user.Id);
    }

    public async Task<UserDto> RejectSellerAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new Exception("Không tìm thấy user");

        if (user.Role != UserRole.SELLER)
            throw new Exception("User không phải là seller");

        user.ApprovalStatus = Core.Enums.ApprovalStatus.REJECTED;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(user.Id);
    }

    public async Task<UserDto> ToggleActiveAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new Exception("Không tìm thấy user");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(user.Id);
    }
}
