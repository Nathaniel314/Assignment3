using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Rider_and_Driver.Controllers
{
    public class DriverOnlyController: Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var username=HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
            {
                var returnUrl= HttpContext.Request.Path + HttpContext.Request.QueryString;
                context.Result = RedirectToAction("Login", "Account", new { returnUrl });
                return;
            }

            if (role != "Driver")
            {
                TempData["Error"] = "Only Drivers can access that page.";
                context.Result = RedirectToAction("Index", "Home");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
