using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Rider_and_Driver.Controllers
{
    public class RiderOnlyController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
                context.Result = RedirectToAction("Login", "Account", new { returnUrl });
                return;
            }

            if (role != "Rider")
            {
                TempData["Error"] = "Only Riders can access that page.";
                context.Result = RedirectToAction("Index", "Home");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
