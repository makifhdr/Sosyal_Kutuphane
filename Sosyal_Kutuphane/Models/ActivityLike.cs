namespace Sosyal_Kutuphane.Models;

public class ActivityLike
{
    public int Id { get; set; }

    public int UserId { get; set; }
    
    public int? RatingId { get; set; }

    public int? ReviewId { get; set; }
}