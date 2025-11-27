using Newtonsoft.Json.Linq;

namespace Sosyal_Kutuphane.Services;

public class TmdbService
{
    private readonly HttpClient _http;
    private readonly string _apiKey = "055cfdc1ac44f7613c9b55b1eef3ebec";

    public TmdbService(HttpClient http)
    {
        _http = http;
    }

    public async Task<JObject> SearchMovie(string query)
    {
        var url = $"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={query}";
        var json = await _http.GetStringAsync(url);
        return JObject.Parse(json);
    }

    public async Task<JObject> GetMovieDetails(int movieId)
    {
        var url = $"https://api.themoviedb.org/3/movie/{movieId}?api_key={_apiKey}&append_to_response=credits";
        var json = await _http.GetStringAsync(url);
        return JObject.Parse(json);
    }
    
    public async Task<string> GetMovieTitle(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return "Unknown Title";
        }

        try
        {
            var json = await _http.GetStringAsync(
                $"https://api.themoviedb.org/3/movie/{id}?api_key={_apiKey}");
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return data?.title ?? "Unknown Title";
        }
        catch
        {
            return "Unknown Title";
        }
    }
}