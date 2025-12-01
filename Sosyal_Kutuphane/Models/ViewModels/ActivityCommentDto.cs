namespace Sosyal_Kutuphane.Models.ViewModels;

public class ActivityCommentDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public byte[] Avatar { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}