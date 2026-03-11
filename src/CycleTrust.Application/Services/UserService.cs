using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CycleTrust.Core.Entities;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.User;

namespace CycleTrust.Application.Services;

public interface IUserService
{
    Task<UserDto> UpdateProfileAsync(long userId, UpdateProfileRequest request);
    Task ChangePasswordAsync(long userId, ChangePasswordRequest request);
    Task<UserDto> UploadAvatarAsync(long userId, IFormFile file);
}

public class UserService : IUserService
{
    private readonly CycleTrustDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserService> _logger;

    public UserService(
        CycleTrustDbContext context, 
        IConfiguration configuration,
        ILogger<UserService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<UserDto> UpdateProfileAsync(long userId, UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Không tìm thấy user");

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName;

        if (request.Phone != null)
            user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(long userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Không tìm thấy user");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new Exception("Mật khẩu hiện tại không đúng");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<UserDto> UploadAvatarAsync(long userId, IFormFile file)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("Không tìm thấy user");

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadPath = Path.Combine("wwwroot", "uploads", "avatars");
        
        Directory.CreateDirectory(uploadPath);

        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            try
            {
                var oldFilePath = Path.Combine("wwwroot", user.AvatarUrl.TrimStart('/'));
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old avatar for user {UserId}", userId);
            }
        }

        user.AvatarUrl = $"/uploads/avatars/{fileName}";
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive,
            ApprovalStatus = user.ApprovalStatus?.ToString(),
            RatingAvg = user.RatingAvg,
            RatingCount = user.RatingCount,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
