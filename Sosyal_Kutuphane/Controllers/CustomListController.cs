using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sosyal_Kutuphane.Data;

namespace Sosyal_Kutuphane.Controllers;

[Authorize]
public class CustomListController : Controller
{
    private readonly ApplicationDbContext _db;

    public CustomListController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /CustomList/Details/5
    public IActionResult Details(int id)
    {
        var list = _db.CustomList
            .Include(l => l.Items)
            .FirstOrDefault(l => l.Id == id);

        if (list == null) return NotFound();

        // Güvenlik: sadece owner görebilir
        var userId = int.Parse(User.FindFirst("UserId").Value);
        if (list.UserId != userId) return Forbid();

        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveItem(int listId, int itemId)
    {
        var userId = int.Parse(User.FindFirst("UserId").Value);
        var list = _db.CustomList.Include(l => l.Items).FirstOrDefault(l => l.Id == listId);
        if (list == null || list.UserId != userId) return Forbid();

        var item = _db.CustomListItem.FirstOrDefault(i => i.Id == itemId && i.CustomListId == listId);
        if (item != null)
        {
            _db.CustomListItem.Remove(item);
            _db.SaveChanges();
        }

        return RedirectToAction("Details", new { id = listId });
    }
}