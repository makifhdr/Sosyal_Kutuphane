namespace Sosyal_Kutuphane.Models;

public class Rating
{
    public int Id { get; set; }

    public User User { get; set; }
    public int UserId { get; set; }
    public string MediaId { get; set; }
    public string MediaType { get; set; }

    public int Score { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public List<ActivityLike> Likes { get; set; }
    public List<ActivityComment> Comments { get; set; }
}