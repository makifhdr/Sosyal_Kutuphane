using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Sosyal_Kutuphane.Data;
using Sosyal_Kutuphane.Models;
using System.Security.Claims;

namespace Sosyal_Kutuphane.Controllers;

public class AccountController : BaseController
{
    private readonly ApplicationDbContext _db;
    
    public AccountController(ApplicationDbContext db)
    {
        _db = db;
    }
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity is { IsAuthenticated: true } || IsLoggedIn())
            return RedirectToAction("Feed", "Activity");

        return View();
    }
    
    [HttpPost]
    public IActionResult Register(string email, string username, string password, string confirmPassword)
    {
        if (IsLoggedIn())
            return RedirectToAction("Feed", "Activity");
        
        if (password != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match.";
            return View();
        }
        
        if (_db.Users.Any(u => u.Email == email))
        {
            ViewBag.Error = "E-mail is already in use.";
            return View();
        }

        var user = new User
        {
            Email = email,
            UserName = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity is { IsAuthenticated: true } || IsLoggedIn())
            return RedirectToAction("Feed", "Activity");
        
        if (TempData["PasswordResetSuccess"] != null)
            ViewBag.Success = TempData["PasswordResetSuccess"];

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = _db.Users.SingleOrDefault(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ViewBag.Error = "E-mail or password is incorrect.";
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("UserId", user.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, "MyCookieAuth");

        await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(identity));

        return RedirectToAction("Feed", "Activity");
    }
    
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("MyCookieAuth");
        return RedirectToAction("Login");
    }
    private bool IsLoggedIn()
    {
        return User.Identity is { IsAuthenticated: true };
    }
    
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        if (User.Identity is { IsAuthenticated: true })
            return RedirectToAction("Feed", "Activity");

        return View();
    }

    [HttpPost]
    public IActionResult ForgotPassword(string email)
    {
        var user = _db.Users.SingleOrDefault(u => u.Email == email);
        if (user == null)
        {
            ViewBag.Error = "No account found with this email.";
            return View();
        }
        
        string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        token = token.Replace("=", "").Replace("+", "").Replace("/", "");
        
        user.ResetToken = token;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        _db.SaveChanges();
        
        var resetLink = Url.Action(
            "ResetPassword",
            "Account",
            new { uid = user.Id, token = token },
            protocol: Request.Scheme
        );
        
        SendResetEmail(user.Email, resetLink);

        ViewBag.Success = "A password reset link has been sent to your email.";
        return View();
    }
    
    [HttpGet]
    public IActionResult ResetPassword(int uid, string token)
    {
        var user = _db.Users.SingleOrDefault(u => u.Id == uid && u.ResetToken == token);

        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            return Content("Invalid or expired token.");

        ViewBag.UserId = uid;
        ViewBag.Token = token;

        return View();
    }

    [HttpPost]
    public IActionResult ResetPassword(int uid, string token, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match.";
            return View();
        }

        var user = _db.Users.SingleOrDefault(u => u.Id == uid && u.ResetToken == token);

        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            ViewBag.Error = "Invalid or expired reset link.";
            return View();
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        _db.SaveChanges();

        TempData["PasswordResetSuccess"] = "Your password has been reset successfully. Please sign in.";

        return RedirectToAction("Login", "Account");
    }
    
    public void SendResetEmail(string email, string resetLink)
    {
        using (var client = new SmtpClient("smtp.gmail.com", 587))
        {
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(
                "makifhidir2861@gmail.com", "fmwc earl pskx iqqm");

            var mail = new MailMessage();
            mail.From = new MailAddress("makifhidir2861@gmail.com", "Social Library Support");
            mail.To.Add(email);
            mail.Subject = "Password Reset";
            mail.Body = $"Click the link to reset your password:\n\n{resetLink}";
            mail.IsBodyHtml = false;

            client.Send(mail);
        }
    }
}