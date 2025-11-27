using Microsoft.AspNetCore.Mvc;
using Sosyal_Kutuphane.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sosyal_Kutuphane.Models;
using Sosyal_Kutuphane.Services;

namespace Sosyal_Kutuphane.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _db;
    
    private readonly TmdbService tmdb;
    private readonly GoogleBooksService gbooks;

    public ProfileController(ApplicationDbContext db,TmdbService tmdb, GoogleBooksService gbooks)
    {
        _db = db;
        this.tmdb = tmdb;
        this.gbooks = gbooks;
    }

    public async Task<IActionResult> Index(int? id)
    {
        int currentUserId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
        int targetUserId = id ?? currentUserId;

        var user = _db.Users
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .Include(u => u.MediaList)
            .Include(u => u.CustomLists)
            .FirstOrDefault(u => u.Id == targetUserId);

        if (user == null)
            return NotFound();

        ViewBag.IsOwner = targetUserId == currentUserId;

        // Takip kontrolü
        ViewBag.IsFollowing = _db.Follow.Any(f =>
            f.FollowerId == currentUserId && f.FollowingId == targetUserId);

        // --- SON AKTİVİTELER ---

        // Yorumlar
        var reviews = _db.Reviews
            .Where(r => r.UserId == targetUserId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .ToList();

        // Ratingler
        var ratings = _db.Ratings
            .Where(r => r.UserId == targetUserId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .ToList();

        // API çağrıları (film mü kitap mı bakıyoruz)
        ViewBag.RecentRatings = await Task.WhenAll(ratings.Select(async x =>
        {
            string title = x.MediaType == "movie"
                ? await tmdb.GetMovieTitle(x.MediaId)
                : await gbooks.GetBookTitle(x.MediaId);

            return new
            {
                x.Id,
                x.Score,
                x.MediaType,
                x.CreatedAt,
                Title = title
            };
        }));

        ViewBag.RecentReviews = await Task.WhenAll(reviews.Select(async x =>
        {
            string title = x.MediaType == "movie"
                ? await tmdb.GetMovieTitle(x.MediaId)
                : await gbooks.GetBookTitle(x.MediaId);

            return new
            {
                x.Id,
                x.Content,
                x.MediaType,
                x.CreatedAt,
                Title = title
            };
        }));

        return View(user);
    }


    [HttpGet]
    public IActionResult Edit()
    {
        int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
        var user = _db.Users.First(u => u.Id == userId);

        return View(user);
    }

    [HttpPost]
    public IActionResult Edit(string username, string bio, IFormFile? avatar)
    {
        int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
        var user = _db.Users.First(u => u.Id == userId);

        if (!string.IsNullOrWhiteSpace(username))
            user.UserName = username;

        user.Bio = bio;

        // Avatar değişmişse
        if (avatar != null)
        {
            using (var ms = new MemoryStream())
            {
                avatar.CopyTo(ms);
                user.Avatar = ms.ToArray();
            }
        }

        _db.SaveChanges();
        return RedirectToAction("Index");
    }
    
    [HttpGet]
    public IActionResult CreateList()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CreateList(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ViewBag.Error = "List name cannot be empty.";
            return View();
        }

        int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

        var list = new CustomList
        {
            Name = name,
            UserId = userId
        };

        _db.CustomList.Add(list);
        _db.SaveChanges();

        return RedirectToAction("Index");
    }
    
    [HttpPost]
    public IActionResult ToggleFollow(int targetUserId)
    {
        var targetUser = _db.Users.FirstOrDefault(u => u.Id == targetUserId);
        if (targetUser == null)
        {
            TempData["Error"] = "User not found!";
            return RedirectToAction("Index", "Home");
        }
        
        // Giriş yapan kullanıcıyı al
        var currentUserId = int.Parse(User.FindFirst("UserId").Value);

        // Kendi kendini takip etmeyi engelle
        if (currentUserId == targetUserId)
            return RedirectToAction("Index", new { id = targetUserId });

        // İlişki zaten var mı?
        var existingFollow = _db.Follow
            .FirstOrDefault(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);

        if (existingFollow != null)
        {
            // ❌ Zaten takip ediyorsa -> takipten çık
            _db.Follow.Remove(existingFollow);
        }
        else
        {
            // ✔ Takip etmiyorsa -> yeni takip kaydı ekle
            var follow = new Follow
            {
                FollowerId = currentUserId,
                FollowingId = targetUserId
            };

            _db.Follow.Add(follow);
        }

        _db.SaveChanges();
        
        return RedirectToAction("Index", new { id = targetUserId });
    }
}