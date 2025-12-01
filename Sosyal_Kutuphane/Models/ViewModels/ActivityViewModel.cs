namespace Sosyal_Kutuphane.Models.ViewModels
{
    public class ActivityViewModel
    {
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public byte[] Avatar { get; set; }

        public string MediaId { get; set; }
        public string MediaType { get; set; }
        public string MediaTitle { get; set; }
        public string MediaPosterUrl { get; set; }

        public string ActivityType { get; set; } // rating | review

        public int? RatingScore { get; set; }
        public string ReviewExcerpt { get; set; }

        public int LikeCount { get; set; }
        public int CommentCount { get; set; }

        public bool UserLiked { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string FullReview { get; set; }
        
        public bool IsLiked { get; set; }
        
        public List<ActivityCommentDto> Comments { get; set; } = new();
    }
}