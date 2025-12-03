namespace Sosyal_Kutuphane.Models.ViewModels
{
    public class ShowcaseItemViewModel
    {
        public string MediaId { get; set; }
        public string MediaType { get; set; }

        public string Title { get; set; }
        public string PosterUrl { get; set; }

        public double? AverageRating { get; set; }
        public int RatingCount { get; set; }

        public int ReviewCount { get; set; }
        public int LibraryCount { get; set; }
        public int CustomListCount { get; set; }

        public int PopularityScore =>
            ReviewCount + LibraryCount + CustomListCount;
    }
}