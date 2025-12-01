using Newtonsoft.Json.Linq;

namespace Sosyal_Kutuphane.Models.ViewModels
{
    public class AdvancedSearchViewModel
    {
        public string Query { get; set; }
        public string Type { get; set; }

        public string Genre { get; set; }
        public int? Year { get; set; }
        public int? MinRating { get; set; }

        public List<JObject> MovieResults { get; set; } = new();
        public List<JObject> BookResults { get; set; } = new();
    }
}