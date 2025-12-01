namespace Sosyal_Kutuphane.Models.ViewModels;

public class ActivityCommentViewModel
{
    public int ActivityId { get; set; }
    public string ActivityType { get; set; }
    public string Content { get; set; } = string.Empty;
}