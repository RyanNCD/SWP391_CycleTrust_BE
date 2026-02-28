namespace CycleTrust.Application.DTOs.Review;

public class ReviewDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewRequest
{
    public long OrderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
