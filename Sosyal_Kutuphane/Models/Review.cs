namespace Sosyal_Kutuphane.Models;

public class Review
{
    public int Id { get; set; }
    
    public User User { get; set; }
    public int UserId { get; set; }
    
    public string MediaId { get; set; }
    public string MediaType { get; set; }

    public string Content { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public List<ActivityLike> Likes { get; set; }
    public List<ActivityComment> Comments { get; set; }
}