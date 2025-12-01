namespace Sosyal_Kutuphane.Models.ViewModels
{
    public class ShowcaseItemViewModel
    {
        public string MediaId { get; set; }
        public string MediaType { get; set; } // "movie" | "book"

        public string Title { get; set; }
        public string PosterUrl { get; set; }

        public double? AverageRating { get; set; }
        public int RatingCount { get; set; }

        public int ReviewCount { get; set; }
        public int LibraryCount { get; set; }      // UserMedia sayısı
        public int CustomListCount { get; set; }   // CustomListItem sayısı

        public int PopularityScore =>
            ReviewCount + LibraryCount + CustomListCount;
    }

    public class HomeShowcaseViewModel
    {
        public List<ShowcaseItemViewModel> TopRated { get; set; } = new();
        public List<ShowcaseItemViewModel> MostPopular { get; set; } = new();
    }
}