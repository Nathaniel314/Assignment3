using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Rider_and_Driver.Data;
using Rider_and_Driver.Models;

namespace Rider_and_Driver.Controllers
{
    public class DriverOnlyController: Controller
    {
        private readonly ApplicationDbContext _db;

        public DriverOnlyController(ApplicationDbContext db)
        {
            _db = db;
        }
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
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTrip(Trip trip)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            trip.DriverUserId = userId.Value;

            ModelState.Remove("DriverUserId");

            if (!ModelState.IsValid)
            {
                return View("Index", trip);
            }

            trip.Status = "Active";
            trip.CreatedAt = DateTime.Now;

            _db.Trips.Add(trip);
            _db.SaveChanges();

            TempData["Success"] = "Ride added successfully.";

            return RedirectToAction("Index");
        }
    }
}
