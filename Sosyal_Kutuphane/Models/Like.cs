namespace Sosyal_Kutuphane.Models;

public class Like
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? ReviewId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}