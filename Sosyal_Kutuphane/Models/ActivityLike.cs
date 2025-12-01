namespace Sosyal_Kutuphane.Models;

public class ActivityLike
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }
    
    public int? RatingId { get; set; }
    public Rating Rating { get; set; }

    public int? ReviewId { get; set; }
    public Review Review { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}