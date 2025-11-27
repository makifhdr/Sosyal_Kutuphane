namespace Sosyal_Kutuphane.Services;

using Newtonsoft.Json.Linq;

public class GoogleBooksService
{
    private readonly HttpClient _http;

    public GoogleBooksService(HttpClient http)
    {
        _http = http;
    }

    public async Task<JObject> SearchBooks(string query)
    {
        var url = $"https://www.googleapis.com/books/v1/volumes?q={query}&langRestrict=en&printType=books";
        var json = await _http.GetStringAsync(url);
        return JObject.Parse(json);
    }
    
    public async Task<JObject> GetBookDetails(string bookId)
    {
        try
        {
            var url = $"https://www.googleapis.com/books/v1/volumes/{bookId}";
            var json = await _http.GetStringAsync(url);
            return JObject.Parse(json);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Google Books API error: {ex.Message}");
            return null;
        }
    }
    
    public async Task<string> GetBookTitle(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "Unknown Title";

        try
        {
            var json = await _http.GetStringAsync($"https://www.googleapis.com/books/v1/volumes/{id}");
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return data?.volumeInfo?.title ?? "Unknown Title";
        }
        catch
        {
            return "Unknown Title";
        }
    }
}