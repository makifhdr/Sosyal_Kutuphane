using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Sosyal_Kutuphane.Data;
using Sosyal_Kutuphane.Models;
using Sosyal_Kutuphane.Models.ViewModels;
using Sosyal_Kutuphane.Services;

namespace Sosyal_Kutuphane.Controllers;

public class ActivityController : Controller
{
    private readonly ApplicationDbContext _db;
    
    private readonly TmdbService _tmdb;
    private readonly GoogleBooksService _books;

    public ActivityController(ApplicationDbContext db, TmdbService tmdb, GoogleBooksService books)
    {
        _db = db;
        _tmdb = tmdb;
        _books = books;
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
    
    public class LikeRequest
    {
        public int ActivityId { get; set; }
        public string ActivityType { get; set; }
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleLikeAjax([FromBody] LikeRequest req)
    {
        int userId = int.Parse(User.FindFirst("UserId").Value);

        ActivityLike like;

        if (req.ActivityType == "rating")
            like = _db.ActivityLike.FirstOrDefault(x => x.UserId == userId && x.RatingId == req.ActivityId);
        else
            like = _db.ActivityLike.FirstOrDefault(x => x.UserId == userId && x.ReviewId == req.ActivityId);

        bool isLiked = false;

        if (like != null)
        {
            _db.ActivityLike.Remove(like);
            isLiked = false;
        }
        else
        {
            _db.ActivityLike.Add(new ActivityLike
            {
                UserId = userId,
                RatingId = req.ActivityType == "rating" ? req.ActivityId : null,
                ReviewId = req.ActivityType == "review" ? req.ActivityId : null
            });

            isLiked = true;
        }

        _db.SaveChanges();

        int likeCount = _db.ActivityLike.Count(l =>
            (req.ActivityType == "rating" && l.RatingId == req.ActivityId) ||
            (req.ActivityType == "review" && l.ReviewId == req.ActivityId)
        );

        return Json(new
        {
            success = true,
            likeCount,
            isLiked
        });
    }
    
    [HttpPost]
    public IActionResult ToggleReviewLikeAjax([FromBody] ReviewLikeRequest req)
    {
        int userId = int.Parse(User.FindFirst("UserId").Value);

        var existing = _db.Likes
            .FirstOrDefault(l => l.UserId == userId && l.ReviewId == req.ReviewId);

        bool isLiked;

        if (existing != null)
        {
            _db.Likes.Remove(existing);
            isLiked = false;
        }
        else
        {
            _db.Likes.Add(new Like
            {
                UserId = userId,
                ReviewId = req.ReviewId
            });
            isLiked = true;
        }

        _db.SaveChanges();

        int likeCount = _db.Likes.Count(l => l.ReviewId == req.ReviewId);

        return Json(new
        {
            success = true,
            isLiked,
            likeCount
        });
    }

    public class ReviewLikeRequest
    {
        public int ReviewId { get; set; }
    }
    
    [HttpGet]
    public IActionResult AddComment(int activityId, string activityType)
    {
        if (activityType != "rating" && activityType != "review")
            return BadRequest("Invalid activity type.");

        var vm = new ActivityCommentViewModel
        {
            ActivityId = activityId,
            ActivityType = activityType
        };

        return View(vm);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddComment(ActivityCommentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = int.Parse(User.FindFirst("UserId").Value);

        var comment = new ActivityComment
        {
            UserId = userId,
            Content = model.Content,
            CreatedAt = DateTime.UtcNow
        };

        if (model.ActivityType == "rating")
            comment.RatingId = model.ActivityId;
        else if (model.ActivityType == "review")
            comment.ReviewId = model.ActivityId;
        else
            return BadRequest("Invalid activity type.");

        _db.ActivityComment.Add(comment);
        _db.SaveChanges();

        return RedirectToAction("Feed", "Activity");
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteComment(int id)
    {
        var userId = int.Parse(User.FindFirst("UserId").Value);

        var comment = _db.ActivityComment.FirstOrDefault(c => c.Id == id);

        if (comment == null)
            return NotFound();

        if (comment.UserId != userId)
            return Forbid();

        _db.ActivityComment.Remove(comment);
        _db.SaveChanges();

        return Redirect(Request.Headers["Referer"].ToString());
    }
    
    [Authorize]
    public async Task<IActionResult> Feed(int page = 1, int pageSize = 10)
    {
        var currentUserId = int.Parse(User.FindFirst("UserId").Value);
        
        var followingIds = await _db.Follow
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        if (followingIds.Count == 0)
        {
            return View(new PagedFeedViewModel());
        }
        
        var ratingsList = await _db.Ratings
            .Where(r => followingIds.Contains(r.UserId))
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var reviewsList = await _db.Reviews
            .Where(rv => followingIds.Contains(rv.UserId))
            .Include(rv => rv.User)
            .OrderByDescending(rv => rv.CreatedAt)
            .ToListAsync();
        
        var ratingIds = ratingsList.Select(r => r.Id).ToList();
        var reviewIds = reviewsList.Select(rv => rv.Id).ToList();

        var likesForRatings = await _db.ActivityLike
            .Where(l => l.RatingId != null && ratingIds.Contains(l.RatingId.Value))
            .GroupBy(l => l.RatingId.Value)
            .Select(g => new { RatingId = g.Key, Count = g.Count() })
            .ToListAsync();

        var likesForReviews = await _db.ActivityLike
            .Where(l => l.ReviewId != null && reviewIds.Contains(l.ReviewId.Value))
            .GroupBy(l => l.ReviewId.Value)
            .Select(g => new { ReviewId = g.Key, Count = g.Count() })
            .ToListAsync();

        var likeCountByRating = likesForRatings.ToDictionary(x => x.RatingId, x => x.Count);
        var likeCountByReview = likesForReviews.ToDictionary(x => x.ReviewId, x => x.Count);
        
        var mediaKeys = ratingsList
            .Select(r => (r.MediaType, r.MediaId))
            .Concat(reviewsList.Select(rv => (rv.MediaType, rv.MediaId)))
            .Distinct()
            .ToList();
        
        var mediaMeta = new Dictionary<(string mediaType, string mediaId), (string title, string posterUrl)>();

        foreach (var (mediaType, mediaId) in mediaKeys)
        {
            try
            {
                if (mediaType == "movie")
                {
                    var movie = await _tmdb.GetMovieDetails(mediaId);
                    var title = movie["title"]?.ToString() ?? "Unknown movie";
                    var poster = movie["poster_path"] != null
                        ? $"https://image.tmdb.org/t/p/w500{movie["poster_path"]}"
                        : "/images/no-poster.png";
                    
                    mediaMeta[(mediaType, mediaId)] = (title, poster);
                }
                else
                {
                    JObject book = await _books.GetBookDetails(mediaId);
                    var title = book["volumeInfo"]?["title"]?.ToString() ?? "Unknown book";
                    var poster = book["volumeInfo"]?["imageLinks"]?["thumbnail"]?.ToString() ?? "/images/no-cover.png";
                    
                    mediaMeta[(mediaType, mediaId)] = (title, poster);
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.Message);
                mediaMeta[(mediaType, mediaId)] = ("Unknown title", "/images/no-cover.png");
            }
        }
        
        
        var activities = new List<ActivityViewModel>();

        foreach (var r in ratingsList)
        {
            mediaMeta.TryGetValue((r.MediaType, r.MediaId), out var meta);
            
            var userId = int.Parse(User.FindFirst("UserId").Value);
            
            var liked = _db.ActivityLike.Any(l =>
                l.UserId == userId && l.RatingId == r.Id
            );
            
            activities.Add(new ActivityViewModel
            {
                ActivityId = r.Id,
                UserId = r.UserId,
                UserName = r.User?.UserName ?? "Unknown",
                Avatar = r.User?.Avatar,
                MediaId = r.MediaId,
                MediaType = r.MediaType,
                MediaTitle = meta.title,
                MediaPosterUrl = meta.posterUrl,
                ActivityType = "rating",
                RatingScore = r.Score,
                CreatedAt = r.CreatedAt,
                LikeCount = likeCountByRating.ContainsKey(r.Id) ? likeCountByRating[r.Id] : 0,
                CommentCount = _db.ActivityComment.Count(c => c.RatingId == r.Id),
                Comments = _db.ActivityComment
                    .Where(c => c.RatingId == r.Id)
                    .Select(c => new ActivityCommentDto
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        UserName = c.User.UserName,
                        Avatar = c.User.Avatar,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt
                    }).ToList(),
                IsLiked = liked
            });
        }

        foreach (var rv in reviewsList)
        {
            
            mediaMeta.TryGetValue((rv.MediaType, rv.MediaId), out var meta);
            
            var userId = int.Parse(User.FindFirst("UserId").Value);
            
            var liked = _db.ActivityLike.Any(l =>
                l.UserId == userId && l.ReviewId == rv.Id
            );
            
            activities.Add(new ActivityViewModel
            {
                ActivityId = rv.Id,
                UserId = rv.UserId,
                UserName = rv.User?.UserName ?? "Unknown",
                Avatar = rv.User?.Avatar,
                MediaId = rv.MediaId,
                MediaType = rv.MediaType,
                MediaTitle = meta.title,
                MediaPosterUrl = meta.posterUrl,
                ActivityType = "review",
                ReviewExcerpt = rv.Content?.Length > 200 ? rv.Content.Substring(0, 200) + "..." : rv.Content,
                FullReview = rv.Content,
                CreatedAt = rv.CreatedAt,
                LikeCount = likeCountByReview.ContainsKey(rv.Id) ? likeCountByReview[rv.Id] : 0,
                CommentCount = _db.ActivityComment.Count(c => c.ReviewId == rv.Id),
                Comments = _db.ActivityComment
                    .Where(c => c.ReviewId == rv.Id)
                    .Select(c => new ActivityCommentDto
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        UserName = c.User.UserName,
                        Avatar = c.User.Avatar,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt
                    }).ToList(),
                IsLiked = liked
            });
        }
        
        var ordered = activities
            .OrderByDescending(a => a.CreatedAt)
            .ToList();
        
        var totalCount = activities.Count;
        
        var pagedActivities = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return View(new PagedFeedViewModel
        {
            Activities = pagedActivities,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }
}