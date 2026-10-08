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
        public IActionResult Index(DateTime? date,
            string? FromLocation,
            string? ToLocation,
            string? SortBy,
            string? SortOrder)
        {
            DateTime selectedDate =
                date ?? DateTime.Today;

            var rides = _db.Trips
                .Include(t => t.Driver)
                .Where(t =>
                    t.DepartureDate.Date == selectedDate.Date &&
                    t.Status == "Active" &&
                    t.SeatsAvailable > 0);
                //.OrderBy(t => t.DepartureTime);
            //Not to call list yet since it must sort first so i commented this line of Code that Mehakpreet did
                //.ToList();

            //Search the Departure Location
            if (!string.IsNullOrWhiteSpace(FromLocation))
            {
                rides = rides.Where(t => t.FromLocation.Contains(FromLocation));
            }

            //Search Destination
            if (!string.IsNullOrWhiteSpace(ToLocation))
            {
                rides = rides.Where(t => t.ToLocation.Contains(ToLocation));
            }

            //Sort Cost
            if (SortBy == "Cost")
            {
                if (SortOrder == "Ascending")
                {
                    rides = rides.OrderBy(t => t.Cost);
                }
                else
                {
                    rides = rides.OrderByDescending (t => t.Cost);
                }
            }

            //Sort Seats available for the ride
            else if (SortBy == "SeatsAvailable")
            {
                if (SortOrder == "Ascending")
                {
                    rides = rides.OrderBy (t => t.SeatsAvailable);
                }
                else
                {
                    rides = rides.OrderByDescending (t => t.SeatsAvailable);
                }
            }

            //Default Sorting by Departure Time
            else
            {
                rides = rides.OrderBy(t => t.DepartureDate);
            }

            var ridesList = rides.ToList();
                

            ViewBag.SelectedDate = selectedDate;
            ViewBag.FromLocation = FromLocation; 
            ViewBag.ToLocation = ToLocation;
            ViewBag.SortBy = SortBy; 
            ViewBag.SortOrder = SortOrder;
            

            return View(ridesList);
        }
    }
}