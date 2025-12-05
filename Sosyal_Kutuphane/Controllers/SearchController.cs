using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Sosyal_Kutuphane.Data;
using Sosyal_Kutuphane.Models.ViewModels;
using Sosyal_Kutuphane.Services;

namespace Sosyal_Kutuphane.Controllers;

public class SearchController : Controller
{
    private readonly TmdbService _tmdb;
    private readonly GoogleBooksService _books;
    
    private readonly ApplicationDbContext _db;

    public SearchController(TmdbService tmdb, GoogleBooksService books, ApplicationDbContext db)
    {
        _tmdb = tmdb;
        _books = books;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vm = new ExploreViewModel
        {
            TopRated = await GetTopRatedAsync(12),
            MostPopular = await GetMostPopularAsync(12)
        };
        
        
        return View(vm);
    }
    
    private async Task<List<ShowcaseItemViewModel>> GetTopRatedAsync(int limit)
    {
        var grouped = await _db.Ratings
            .GroupBy(r => new { r.MediaId, r.MediaType })
            .Select(g => new
            {
                g.Key.MediaId,
                g.Key.MediaType,
                Avg = g.Average(x => x.Score),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Avg)
            .ThenByDescending(x => x.Count)
            .Take(limit)
            .ToListAsync();

        var results = new List<ShowcaseItemViewModel>();

        foreach (var g in grouped)
        {
            string title;
            string poster;

            if (g.MediaType == "movie")
            {
                JObject movie = await _tmdb.GetMovieDetails(g.MediaId);
                title = movie["title"]?.ToString() ?? "Unknown movie";
                poster = movie["poster_path"] != null
                    ? $"https://image.tmdb.org/t/p/w342{movie["poster_path"]}"
                    : "/images/no-poster.png";
            }
            else
            {
                JObject book = await _books.GetBookDetails(g.MediaId);
                title = book["volumeInfo"]?["title"]?.ToString() ?? "Unknown book";
                poster = book["volumeInfo"]?["imageLinks"]?["thumbnail"]?.ToString()
                    ?? "/images/no-cover.png";
            }
            
            int reviewCount = await _db.Reviews
                .CountAsync(r => r.MediaId == g.MediaId && r.MediaType == g.MediaType);

            int libraryCount = await _db.UserMedia
                .CountAsync(m => m.MediaId == g.MediaId && m.MediaType == g.MediaType);

            int customListCount = await _db.CustomListItem
                .CountAsync(c => c.MediaId == g.MediaId && c.MediaType == g.MediaType);

            results.Add(new ShowcaseItemViewModel
            {
                MediaId = g.MediaId,
                MediaType = g.MediaType,
                Title = title,
                PosterUrl = poster,
                AverageRating = Math.Round(g.Avg, 1),
                RatingCount = g.Count,
                ReviewCount = reviewCount,
                LibraryCount = libraryCount,
                CustomListCount = customListCount
            });
        }

        return results;
    }
    
    private async Task<List<ShowcaseItemViewModel>> GetMostPopularAsync(int limit)
    {
        var popularity = await _db.Reviews
            .GroupBy(r => new { r.MediaId, r.MediaType })
            .Select(g => new
            {
                g.Key.MediaId,
                g.Key.MediaType,
                ReviewCount = g.Count()
            })
            .ToListAsync();
        
        var libStats = await _db.UserMedia
            .GroupBy(m => new { m.MediaId, m.MediaType })
            .Select(g => new
            {
                g.Key.MediaId,
                g.Key.MediaType,
                LibraryCount = g.Count()
            })
            .ToListAsync();
        
        var listStats = await _db.CustomListItem
            .GroupBy(c => new { c.MediaId, c.MediaType })
            .Select(g => new
            {
                g.Key.MediaId,
                g.Key.MediaType,
                CustomListCount = g.Count()
            })
            .ToListAsync();
        
        // 🔥 ÖNEMLİ: Tüm kaynaklardan birleşik unique medya listesi
        var allMedia = popularity
            .Select(x => new { x.MediaId, x.MediaType })
            .Union(libStats.Select(x => new { x.MediaId, x.MediaType }))
            .Union(listStats.Select(x => new { x.MediaId, x.MediaType }))
            .Distinct()
            .ToList();
        
        var items = allMedia
            .Select(m => new
            {
                m.MediaId,
                m.MediaType,
                ReviewCount = popularity.FirstOrDefault(x => x.MediaId == m.MediaId && x.MediaType == m.MediaType)?.ReviewCount ?? 0,
                LibraryCount = libStats.FirstOrDefault(x => x.MediaId == m.MediaId && x.MediaType == m.MediaType)?.LibraryCount ?? 0,
                CustomListCount = listStats.FirstOrDefault(x => x.MediaId == m.MediaId && x.MediaType == m.MediaType)?.CustomListCount ?? 0
            })
            .Select(x => new
            {
                x.MediaId,
                x.MediaType,
                x.ReviewCount,
                x.LibraryCount,
                x.CustomListCount,
                Score = x.ReviewCount + x.LibraryCount + x.CustomListCount
            })
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .ToList();

        var results = new List<ShowcaseItemViewModel>();

        foreach (var g in items)
        {
            string title;
            string poster;

            if (g.MediaType == "movie")
            {
                    JObject movie = await _tmdb.GetMovieDetails(g.MediaId);
                    title = movie["title"]?.ToString() ?? "Unknown movie";
                    poster = movie["poster_path"] != null
                        ? $"https://image.tmdb.org/t/p/w342{movie["poster_path"]}"
                        : "/images/no-poster.png";
                
            }
            else
            {
                JObject book = await _books.GetBookDetails(g.MediaId);
                title = book["volumeInfo"]?["title"]?.ToString() ?? "Unknown book";
                poster = book["volumeInfo"]?["imageLinks"]?["thumbnail"]?.ToString()
                    ?? "/images/no-cover.png";
            }

            results.Add(new ShowcaseItemViewModel
            {
                MediaId = g.MediaId,
                MediaType = g.MediaType,
                Title = title,
                PosterUrl = poster,
                RatingCount = await _db.Ratings.CountAsync(r => r.MediaId == g.MediaId && r.MediaType == g.MediaType),
                ReviewCount = g.ReviewCount,
                LibraryCount = g.LibraryCount,
                CustomListCount = g.CustomListCount
            });
        }
        return results;
    }
    
    [HttpGet]
    public async Task<IActionResult> AdvancedSearch(
        string query, 
        string type,
        string genre = "",
        int? year = null,
        int? minRating = null
    )
    {
        
        if (string.IsNullOrWhiteSpace(query))
            return RedirectToAction("Index");

        var vm = new AdvancedSearchViewModel
        {
            Query = query,
            Type = type,
            Genre = genre,
            Year = year,
            MinRating = minRating,
        };
        
        if (type == "movie")
        {
            var moviesData = await _tmdb.SearchMovie(query);
            var movies = moviesData["results"]?
                .Select(r => (JObject)r)
                .ToList() ?? new List<JObject>();
            
            if (!string.IsNullOrWhiteSpace(genre))
                movies = movies
                    .Where(m => m["genre_ids"] != null &&
                                m["genre_ids"].Any(g => g.ToString() == genre))
                    .ToList();
            
            if (year != null)
                movies = movies
                    .Where(m => m["release_date"]?.ToString().StartsWith(year.ToString())??false)
                    .ToList();
            
            var ratingLookup = _db.Ratings
                .GroupBy(r => r.MediaId)
                .Select(g => new {
                    MediaId = g.Key,
                    AvgScore = g.Average(x => x.Score)
                })
                .ToDictionary(x => x.MediaId, x => x.AvgScore);

            if (minRating != null)
            {
                movies = movies
                    .Where(m =>
                    {
                        string id = m["id"]?.ToString();
                        if (id == null) return false;

                        if (ratingLookup.TryGetValue(id, out double dbRating))
                        {
                            return dbRating >= minRating.Value;
                        }

                        return false;
                    })
                    .ToList();
            }

            vm.MovieResults = movies;
            return View("AdvancedResults", vm);
        }
        
        if (type == "book")
        {
            var booksData = await _books.SearchBooks(query);
            var books = booksData["items"]?
                .Select(i => (JObject)i)
                .ToList() ?? new List<JObject>();
            
            if (!string.IsNullOrEmpty(genre))
            {
                books = books.Where(b =>
                {
                    var cats = b["volumeInfo"]?["categories"];
                    if (cats == null) return false;

                    return cats.Any(c =>
                        c.ToString().ToLower().Contains(genre.ToLower())
                    );
                }).ToList();
            }
            
            if (year != null)
                books = books.Where(b =>
                {
                    var d = (string?)b["volumeInfo"]?["publishedDate"];
                    return d != null && d.StartsWith(year.ToString());
                }).ToList();
            
            var ratingLookup = _db.Ratings
                .GroupBy(r => r.MediaId)
                .Select(g => new {
                    MediaId = g.Key,
                    AvgScore = g.Average(x => x.Score)
                })
                .ToDictionary(x => x.MediaId, x => x.AvgScore);

            if (minRating != null)
                books = books
                    .Where(b =>
                    {
                        string id = b["id"]?.ToString();
                        if (id == null) return false;

                        if (ratingLookup.TryGetValue(id, out double dbRating))
                        {
                            return dbRating >= minRating.Value;
                        }

                        return false;
                    })
                    .ToList();

            vm.BookResults = books;
            return View("AdvancedResults", vm);
        }

        return RedirectToAction("Index");
    }
}