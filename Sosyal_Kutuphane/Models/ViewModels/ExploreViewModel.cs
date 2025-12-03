using Newtonsoft.Json.Linq;

namespace Sosyal_Kutuphane.Models.ViewModels
{
    public class ExploreViewModel
    {
        public List<ShowcaseItemViewModel> TopRated { get; set; } = new();
        public List<ShowcaseItemViewModel> MostPopular { get; set; } = new();
    }
}