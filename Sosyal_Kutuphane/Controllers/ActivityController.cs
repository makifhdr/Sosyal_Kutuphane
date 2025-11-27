using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sosyal_Kutuphane.Data;
using Sosyal_Kutuphane.Models;

namespace Sosyal_Kutuphane.Controllers;

public class ActivityController : Controller
{
    private readonly ApplicationDbContext _db;

    public ActivityController(ApplicationDbContext db)
    {
        _db = db;
    }
    
    [HttpPost]
    public IActionResult AddReview(string mediaId, string mediaType, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return BadRequest();

        var userId = int.Parse(User.FindFirst("UserId")!.Value);

        var review = new Review
        {
            UserId = userId,
            MediaId = mediaId,
            MediaType = mediaType,
            Content = content
        };

        _db.Reviews.Add(review);
        _db.SaveChanges();

        return Redirect(Request.Headers["Referer"].ToString());
    }
    
    [HttpPost]
    public IActionResult ToggleLike(int reviewId)
    {
        var userId = int.Parse(User.FindFirst("UserId")!.Value);

        var existing = _db.Likes.SingleOrDefault(l => l.UserId == userId && l.ReviewId == reviewId);

        if (existing == null)
        {
            _db.Likes.Add(new Like { UserId = userId, ReviewId = reviewId });
        }
        else
        {
            _db.Likes.Remove(existing);
        }

        _db.SaveChanges();

        return Redirect(Request.Headers["Referer"].ToString());
    }
    
    [HttpPost]
    public IActionResult Rate(string mediaId, string mediaType, int score)
    {
        if (score < 1 || score > 10)
            return BadRequest();

        var userId = int.Parse(User.FindFirst("UserId")!.Value);

        var existing = _db.Ratings.SingleOrDefault(r =>
            r.UserId == userId && r.MediaId == mediaId && r.MediaType == mediaType);

        if (existing == null)
        {
            _db.Ratings.Add(new Rating
            {
                UserId = userId,
                MediaId = mediaId,
                MediaType = mediaType,
                Score = score
            });
        }
        else
        {
            existing.Score = score;
        }

        _db.SaveChanges();

        return Redirect(Request.Headers["Referer"].ToString());
    }
    
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteReview(int reviewId)
    {
        var userId = int.Parse(User.FindFirst("UserId")!.Value);
        
        var review = _db.Reviews.SingleOrDefault(r => r.Id == reviewId);

        if (review == null)
            return NotFound();
        
        if (review.UserId != userId)
            return Forbid();

        _db.Reviews.Remove(review);
        _db.SaveChanges();

        return Redirect(Request.Headers["Referer"].ToString());
    }
    
    [Authorize]
    [HttpGet]
    public IActionResult EditReview(int reviewId)
    {
        var userId = int.Parse(User.FindFirst("UserId")!.Value);

        var review = _db.Reviews.SingleOrDefault(r => r.Id == reviewId);

        if (review == null)
            return NotFound();

        if (review.UserId != userId)
            return Forbid();

        return View(review); // Views/Activity/EditReview.cshtml
    }
    
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditReview(int reviewId, string content)
    {
        var userId = int.Parse(User.FindFirst("UserId")!.Value);

        var review = _db.Reviews.SingleOrDefault(r => r.Id == reviewId);

        if (review == null)
            return NotFound();

        if (review.UserId != userId)
            return Forbid();

        review.Content = content;
        _db.SaveChanges();


        switch (review.MediaType)
        {
            case "movie":
                return RedirectToAction("MovieDetails", "Media", new { id = review.MediaId });
            case "book":
                return RedirectToAction("BookDetails", "Media", new { id = review.MediaId });
        }

        return RedirectToAction("Index", "Home");
    }
}