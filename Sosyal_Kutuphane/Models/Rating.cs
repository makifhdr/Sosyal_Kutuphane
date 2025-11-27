namespace Sosyal_Kutuphane.Models;

public class Rating
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public string MediaId { get; set; }
    public string MediaType { get; set; }

    public int Score { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // <-- Eklenen alan
}