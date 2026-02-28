namespace CycleTrust.Application.DTOs.User;

public class UserDto
{
    public long Id { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public decimal RatingAvg { get; set; }
    public int RatingCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
