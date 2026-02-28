using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Core.Enums;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.Auth;

namespace CycleTrust.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly CycleTrustDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(CycleTrustDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (!string.IsNullOrEmpty(request.Email))
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new Exception("Email đã tồn tại");
        }

        if (!string.IsNullOrEmpty(request.Phone))
        {
            if (await _context.Users.AnyAsync(u => u.Phone == request.Phone))
                throw new Exception("Phone đã tồn tại");
        }

        var user = new User
        {
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = Enum.Parse<UserRole>(request.Role, true),
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Token = token
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.EmailOrPhone || u.Phone == request.EmailOrPhone);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new Exception("Email/Phone hoặc mật khẩu không đúng");

        if (!user.IsActive)
            throw new Exception("Tài khoản đã bị khóa");

        var token = GenerateJwtToken(user);
        return new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Token = token
        };
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("email", user.Email ?? ""),
            new Claim("phone", user.Phone ?? "")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
