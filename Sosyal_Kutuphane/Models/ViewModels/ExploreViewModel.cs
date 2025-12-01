using Newtonsoft.Json.Linq;

namespace Sosyal_Kutuphane.Models.ViewModels
{
    public class ExploreViewModel
    {
        public string SearchQuery { get; set; }

        public List<JObject> SearchResultsMovies { get; set; } = new();
        public List<JObject> SearchResultsBooks { get; set; } = new();

        public List<ShowcaseItemViewModel> TopRated { get; set; } = new();
        public List<ShowcaseItemViewModel> MostPopular { get; set; } = new();
    }
}