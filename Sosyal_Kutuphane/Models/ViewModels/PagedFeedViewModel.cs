namespace Sosyal_Kutuphane.Models.ViewModels;

public class PagedFeedViewModel
{
    public List<ActivityViewModel> Activities { get; set; } = new();

    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public bool HasNextPage =>
        CurrentPage * PageSize < TotalCount;
}