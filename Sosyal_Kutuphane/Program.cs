using Microsoft.EntityFrameworkCore;
using Sosyal_Kutuphane.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Sosyal_Kutuphane.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
    });

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

builder.Services.AddHttpClient<TmdbService>();
builder.Services.AddHttpClient<GoogleBooksService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();