using Microsoft.AspNetCore.Mvc;
using Sosyal_Kutuphane.Services;

namespace Sosyal_Kutuphane.Controllers;

public class SearchController : Controller
{
    private readonly TmdbService _tmdb;
    private readonly GoogleBooksService _books;

    public SearchController(TmdbService tmdb, GoogleBooksService books)
    {
        _tmdb = tmdb;
        _books = books;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Results(string query, string type)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            ViewBag.Error = "Please enter a search query.";
            return View("Index");
        }

        if (type == "movie")
        {
            var movies = await _tmdb.SearchMovie(query);
            return View("MovieResults", movies);
        }
        if (type == "book")
        {
            var books = await _books.SearchBooks(query);
            return View("BookResults", books);
        }

        ViewBag.Error = "Unknown search type.";
        return View("Index");
    }
}