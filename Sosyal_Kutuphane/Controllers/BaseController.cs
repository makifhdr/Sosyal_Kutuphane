using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sosyal_Kutuphane.Controllers;

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Layout = User.Identity is { IsAuthenticated: false } ? "_AuthLayout" : "_Layout";

        base.OnActionExecuting(context);
    }
}