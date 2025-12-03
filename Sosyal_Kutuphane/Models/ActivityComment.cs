namespace Sosyal_Kutuphane.Models;

public class ActivityComment
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public string Content { get; set; }

    public int? RatingId { get; set; }

    public int? ReviewId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}