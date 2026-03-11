using CycleTrust.Core.Enums;

namespace CycleTrust.Core.Entities;

public class User : BaseEntity
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public ApprovalStatus? ApprovalStatus { get; set; } // For seller approval
    public decimal RatingAvg { get; set; } = 0.00m;
    public int RatingCount { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    public ICollection<Order> OrdersAsBuyer { get; set; } = new List<Order>();
    public ICollection<Order> OrdersAsSeller { get; set; } = new List<Order>();
    public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
    public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
}
