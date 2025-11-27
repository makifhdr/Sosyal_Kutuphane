namespace Sosyal_Kutuphane.Helpers;

public static class TimeAgo
{
    public static string Format(DateTime date)
    {
        var ts = DateTime.UtcNow - date;

        if (ts.TotalSeconds < 60)
            return $"{(int)ts.TotalSeconds} seconds ago";

        if (ts.TotalMinutes < 60)
            return $"{(int)ts.TotalMinutes} minutes ago";

        if (ts.TotalHours < 24)
            return $"{(int)ts.TotalHours} hours ago";

        if (ts.TotalDays < 30)
            return $"{(int)ts.TotalDays} days ago";

        if (ts.TotalDays < 365)
            return $"{(int)(ts.TotalDays / 30)} months ago";

        return $"{(int)(ts.TotalDays / 365)} years ago";
    }
}
