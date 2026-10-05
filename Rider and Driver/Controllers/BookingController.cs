using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rider_and_Driver.Data;
using Rider_and_Driver.Models;

namespace Rider_and_Driver.Controllers
{
    public class BookingController : RiderOnlyController
    {
        private readonly ApplicationDbContext _db;

        public BookingController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Browse()
        {
            var riderId = HttpContext.Session.GetInt32("UserId");

            var trips = _db.Trips
                .Include(t => t.Driver)
                .Where(t => t.DepartureDate.Date >= DateTime.Now && t.SeatsAvailable > 0 && t.Status== "Active")
                .OrderBy(t => t.DepartureDate)
                .ThenBy(t => t.DepartureTime)
                .ToList();

            var bookedTripIds= _db.Bookings
                .Where(b => b.RiderUserId == riderId && b.Status == "Confirmed")
                .Select(b => b.TripId)
                .ToList();

            ViewBag.BookedTripIds= bookedTripIds;
            return View(trips);
        }

        [HttpGet]
        public IActionResult Book(int id)
        {
            var trip = _db.Trips
                .Include(t => t.Driver)
                .FirstOrDefault(t => t.TripId == id);

            if (trip == null)
                return NotFound();

            if (trip.SeatsAvailable <= 0)
            {
                TempData["Error"] = "Sorry, this trip is full.";
                return RedirectToAction("Browse");
            }

            var riderId = HttpContext.Session.GetInt32("UserId");
            var existing = _db.Bookings
                .Any(b => b.TripId == id && b.RiderUserId == riderId && b.Status == "Confirmed");

            if (existing)
            {
                TempData["Error"] = "You have already booked this Trip";
                return RedirectToAction("MyBookings");
            }

            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Book")]
        public IActionResult BookPost(int id)
        {
            var riderId = HttpContext.Session.GetInt32("UserId");
            if (riderId == null)
                return RedirectToAction("Login", "Account");

            var trip = _db.Trips.FirstOrDefault(t => t.TripId == id);

            if (trip == null)
                return NotFound();

            if (trip.SeatsAvailable <= 0)
            {
                TempData["Error"] = "Sorry, This trip is fully Booked";
                return RedirectToAction("Browser");
            }

            var existing = _db.Bookings
                .Any(b => b.TripId == id && b.RiderUserId == riderId && b.Status == "Confirmed");

            if (existing)
            {
                TempData["Error"] = "You have already Booked this trip";
                return RedirectToAction("MyBookings");
            }

            var booking = new Booking
            {
                TripId = id,
                RiderUserId = riderId.Value,
                TotalCost = trip.Cost,
                Status = "Confirmed",
                BookedAt = DateTime.Now
            };

            trip.SeatsAvailable -= 1;

            _db.Bookings.Add(booking);
            _db.SaveChanges();

            TempData["Success"] = $"Trip Booked! Seat reserved for ${trip.Cost:F2}.";
            return RedirectToAction("MyBookings");
        }
        [HttpGet]
        public IActionResult MyBookings()
        {
            var riderId = HttpContext.Session.GetInt32("UserId");

            var booking = _db.Bookings
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Driver)
                .Where(b => b.RiderUserId == riderId && b.Status== "Confirmed" && b.Trip != null && b.Trip.Status=="Active")
                .OrderByDescending(b => b.BookedAt)
                .ToList();
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var riderId = HttpContext.Session.GetInt32("UserId");

            var booking = _db.Bookings
               .Include(b => b.Trip)
               .FirstOrDefault(b => b.BookingId == id);

            if (booking == null)
                return NotFound();

            if (booking.RiderUserId != riderId)
            {
                TempData["Error"] = "You can only cancel your own booking.";
                return View(MyBookings);
            }

            if (booking.Status == "Cancelled")
            {
                TempData["Error"] = "This booking is already Cancelled";
                return View(MyBookings);
            }

            booking.Status = "Cancelled";
            if (booking.Trip != null && booking.Trip.SeatsAvailable < 10)
            {
                booking.Trip.SeatsAvailable += 1;
            }

            _db.SaveChanges();

            TempData["Success"] = "Booking cancelled";
            return RedirectToAction("MyBookings");
        }
    }
}
