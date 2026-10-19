using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rider_and_Driver.Data;
using Rider_and_Driver.Models;

namespace Rider_and_Driver.Controllers
{
    public class HistoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HistoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        //Displays the trasaction history, shows data based on the users role
        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            //ensure user is loged in
            if (userId == null)
                return RedirectToAction("Login", "Account");

            //If user is Rider, they only see booking they have made
            if (role =="Rider")
            {
                //Queries all bokking where RideruserID matches logged in user
                //loads the trip and driver detials to display
                var bookings = _db.Bookings
                    .Include(b=>b.Trip)
                        .ThenInclude(t => t.Driver)
                    .Where(b => b.RiderUserId == userId)
                    .OrderByDescending(b => b.BookedAt)
                    .ToList();

                //Calculates money spent
                ViewBag.TotalSpent = bookings
                    .Where(b => b.Status == "Confirmed")
                    .Sum(b => b.TotalCost);

                ViewBag.Role = "Rider";
                return View(bookings);
            }

            //If user is a Driver they see their trips.
            if (role == "Driver")
            {
                //Queries all bokking where DriveruserID matches logged in user
                //Ensurs a driver cannot see other drivers bookings
                var bookings = _db.Bookings
                    .Include(b => b.Rider)
                    .Include(b => b.Trip)
                    .Where(b => b.Trip != null && b.Trip.DriverUserId== userId)
                    .OrderByDescending(b => b.BookedAt)
                    .ToList();

                //Callculates money earned from confirmed bookings
                ViewBag.TotalEarned = bookings
                    .Where(b => b.Status == "Confirmed")
                    .Sum(b => b.TotalCost);

                ViewBag.Role = "Driver";
                return View(bookings);
            }

            //fallback, if role doesnt exist
            TempData["Error"] = "Unknown user role.";
            return RedirectToAction("Index", "Home");

        }
    }
}
