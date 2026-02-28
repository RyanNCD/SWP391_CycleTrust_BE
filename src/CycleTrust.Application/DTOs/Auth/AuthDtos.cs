namespace CycleTrust.Application.DTOs.Auth;

public class RegisterRequest
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "BUYER";
}

public class LoginRequest
{
    public string? EmailOrPhone { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public long UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
