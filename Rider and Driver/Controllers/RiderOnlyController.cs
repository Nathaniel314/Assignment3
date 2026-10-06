using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Rider_and_Driver.Data;

namespace Rider_and_Driver.Controllers
{
    public class RiderOnlyController : Controller
    {
        private readonly ApplicationDbContext _db;

        public RiderOnlyController(ApplicationDbContext db)
        {
            _db = db;
        }

        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            var username =
                HttpContext.Session.GetString("Username");

            var role =
                HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
            {
                var returnUrl =
                    HttpContext.Request.Path +
                    HttpContext.Request.QueryString;

                context.Result = RedirectToAction(
                    "Login",
                    "Account",
                    new { returnUrl });

                return;
            }

            if (role != "Rider")
            {
                TempData["Error"] =
                    "Only Riders can access that page.";

                context.Result = RedirectToAction(
                    "Index",
                    "Home");

                return;
            }

            base.OnActionExecuting(context);
        }


        [HttpGet]
        public IActionResult Index(DateTime? date)
        {
            DateTime selectedDate =
                date ?? DateTime.Today;

            var rides = _db.Trips
                .Include(t => t.Driver)
                .Where(t =>
                    t.DepartureDate.Date == selectedDate.Date &&
                    t.Status == "Active" &&
                    t.SeatsAvailable > 0)
                .OrderBy(t => t.DepartureTime)
                .ToList();

            ViewBag.SelectedDate = selectedDate;

            return View(rides);
        }
    }
}