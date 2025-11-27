namespace Sosyal_Kutuphane.Models;

public class UserMedia
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string MediaId { get; set; }
    public string MediaType { get; set; }

    public string Status { get; set; }
}