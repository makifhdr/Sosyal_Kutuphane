using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sosyal_Kutuphane.Data;
using Sosyal_Kutuphane.Models;
using Sosyal_Kutuphane.Services;

namespace Sosyal_Kutuphane.Controllers;

public class MediaController : Controller
{
    
    private readonly TmdbService _tmdb;
    private readonly GoogleBooksService _books;
    
    private readonly ApplicationDbContext _db;

    public MediaController(ApplicationDbContext db, TmdbService tmdb, GoogleBooksService books)
    {
        _db = db;
        _tmdb = tmdb;
        _books = books;
    }

    public async Task<IActionResult> MovieDetails(int id)
    {
        var movie = await _tmdb.GetMovieDetails(id);
        return View(movie);
    }
    
    public async Task<IActionResult> BookDetails(string id)
    {
        var book = await _books.GetBookDetails(id);

        if (book == null)
            return NotFound();

        return View(book);
    } 
    
    
[Authorize]
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult AddToLibrary(string mediaId, string mediaType, string status)
{
    var userId = int.Parse(User.FindFirst("UserId").Value);

    var valid = new[] { "watched", "towatch", "read", "toread" };
    if (!valid.Contains(status)) return BadRequest();

    var existing = _db.UserMedia
        .FirstOrDefault(um => um.UserId == userId && um.MediaId == mediaId && um.MediaType == mediaType);

    if (existing == null)
    {
        _db.UserMedia.Add(new UserMedia
        {
            UserId = userId,
            MediaId = mediaId,
            MediaType = mediaType,
            Status = status
        });
    }
    else
    {
        existing.Status = status;
    }

    _db.SaveChanges();
    return Redirect(Request.Headers["Referer"].ToString());
}

[Authorize]
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult RemoveFromLibrary(string mediaId, string mediaType)
{
    var userId = int.Parse(User.FindFirst("UserId").Value);

    var existing = _db.UserMedia
        .FirstOrDefault(um => um.UserId == userId && um.MediaId == mediaId && um.MediaType == mediaType);

    if (existing != null)
    {
        _db.UserMedia.Remove(existing);
        _db.SaveChanges();
    }

    return Redirect(Request.Headers["Referer"].ToString());
}

[Authorize]
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult AddToCustomList(int listId, string mediaId, string mediaType)
{
    var userId = int.Parse(User.FindFirst("UserId").Value);
    var list = _db.CustomList.FirstOrDefault(l => l.Id == listId && l.UserId == userId);
    if (list == null) return Forbid();

    var existing = _db.CustomListItem
        .FirstOrDefault(i => i.CustomListId == listId && i.MediaId == mediaId && i.MediaType == mediaType);

    if (existing == null)
    {
        _db.CustomListItem.Add(new CustomListItem
        {
            CustomListId = listId,
            MediaId = mediaId,
            MediaType = mediaType
        });
        _db.SaveChanges();
    }

    return Redirect(Request.Headers["Referer"].ToString());
}

[Authorize]
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult RemoveFromCustomList(int listId, string mediaId, string mediaType)
{
    var userId = int.Parse(User.FindFirst("UserId").Value);
    var list = _db.CustomList.FirstOrDefault(l => l.Id == listId && l.UserId == userId);
    if (list == null) return Forbid();

    var item = _db.CustomListItem.FirstOrDefault(i => i.CustomListId == listId && i.MediaId == mediaId && i.MediaType == mediaType);
    if (item != null)
    {
        _db.CustomListItem.Remove(item);
        _db.SaveChanges();
    }

    return Redirect(Request.Headers["Referer"].ToString());
}
}