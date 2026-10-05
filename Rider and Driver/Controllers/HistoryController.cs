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

        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (role =="Rider")
            {
                var bookings = _db.Bookings
                    .Include(b=>b.Trip)
                        .ThenInclude(t => t.Driver)
                    .Where(b => b.RiderUserId == userId)
                    .OrderByDescending(b => b.BookedAt)
                    .ToList();

                ViewBag.TotalSpent = bookings
                    .Where(b => b.Status == "Confirmed")
                    .Sum(b => b.TotalCost);

                ViewBag.Role = "Rider";
                return View(bookings);
            }

            if (role == "Driver")
            {
                var bookings = _db.Bookings
                    .Include(b => b.Rider)
                    .Include(b => b.Trip)
                    .Where(b => b.Trip != null && b.Trip.DriverUserId== userId)
                    .OrderByDescending(b => b.BookedAt)
                    .ToList();

                ViewBag.TotalEarned = bookings
                    .Where(b => b.Status == "Confirmed")
                    .Sum(b => b.TotalCost);

                ViewBag.Role = "Driver";
                return View(bookings);
            }

            TempData["Error"] = "Unknown user role.";
            return RedirectToAction("Index", "Home");

        }
    }
}
