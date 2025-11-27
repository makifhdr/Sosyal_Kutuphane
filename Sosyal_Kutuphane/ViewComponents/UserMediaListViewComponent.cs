using Microsoft.AspNetCore.Mvc;
using Sosyal_Kutuphane.Data;

namespace Sosyal_Kutuphane.ViewComponents;

public class UserMediaListViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public UserMediaListViewComponent(ApplicationDbContext db)
    {
        _db = db;
    }

    public IViewComponentResult Invoke(int userId, string status)
    {
        var items = _db.UserMedia
            .Where(m => m.UserId == userId && m.Status == status)
            .ToList();

        return View(items);
    }
}