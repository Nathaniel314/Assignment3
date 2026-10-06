using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rider_and_Driver.Data;
using Rider_and_Driver.Models;

namespace Rider_and_Driver.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BookingController(ApplicationDbContext db)
        {
            _db = db;
        }


        // =========================
        // BOOK RIDE
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Book(int tripId)
        {
            var username =
                HttpContext.Session.GetString("Username");

            var role =
                HttpContext.Session.GetString("Role");

            var riderUserId =
                HttpContext.Session.GetInt32("UserId");


            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (role != "Rider")
            {
                TempData["Error"] =
                    "Only Riders can book rides.";

                return RedirectToAction(
                    "Index",
                    "Home");
            }


            if (riderUserId == null)
            {
                TempData["Error"] =
                    "Unable to identify rider.";

                return RedirectToAction(
                    "Index",
                    "RiderOnly");
            }


            var trip = _db.Trips
                .FirstOrDefault(t =>
                    t.TripId == tripId);


            if (trip == null)
            {
                TempData["Error"] =
                    "Ride not found.";

                return RedirectToAction(
                    "Index",
                    "RiderOnly");
            }


            if (trip.Status != "Active")
            {
                TempData["Error"] =
                    "This ride is no longer available.";

                return RedirectToAction(
                    "Index",
                    "RiderOnly");
            }


            if (trip.SeatsAvailable <= 0)
            {
                TempData["Error"] =
                    "No seats are available.";

                return RedirectToAction(
                    "Index",
                    "RiderOnly");
            }


            var alreadyBooked =
                _db.Bookings.Any(
                    b =>
                        b.TripId == tripId &&
                        b.RiderUserId ==
                            riderUserId.Value &&
                        b.Status == "Confirmed"
                );


            if (alreadyBooked)
            {
                TempData["Error"] =
                    "You have already booked this ride.";

                return RedirectToAction(
                    "Index",
                    "RiderOnly");
            }


            var booking = new Booking
            {
                TripId = trip.TripId,

                RiderUserId =
                    riderUserId.Value,

                TotalCost =
                    trip.Cost,

                Status =
                    "Confirmed",

                BookedAt =
                    DateTime.Now
            };


            trip.SeatsAvailable--;


            _db.Bookings.Add(booking);

            _db.SaveChanges();


            TempData["Success"] =
                "Ride booked successfully.";


            return RedirectToAction(
                "MyBookings");
        }


        // =========================
        // MY BOOKINGS
        // =========================

        [HttpGet]
        public IActionResult MyBookings()
        {
            var riderUserId =
                HttpContext.Session.GetInt32("UserId");


            if (riderUserId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var bookings = _db.Bookings
                .Include(b => b.Trip)
                .ThenInclude(t => t.Driver)
                .Where(
                    b =>
                        b.RiderUserId ==
                        riderUserId.Value
                )
                .OrderByDescending(
                    b => b.BookedAt)
                .ToList();


            return View(bookings);
        }


        // =========================
        // CANCEL BOOKING
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var riderUserId =
                HttpContext.Session.GetInt32("UserId");


            if (riderUserId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var booking = _db.Bookings
                .Include(b => b.Trip)
                .FirstOrDefault(
                    b =>
                        b.BookingId == id &&
                        b.RiderUserId ==
                            riderUserId.Value
                );


            if (booking == null)
            {
                TempData["Error"] =
                    "Booking not found.";

                return RedirectToAction(
                    "MyBookings");
            }


            if (booking.Status == "Cancelled")
            {
                TempData["Error"] =
                    "This booking is already cancelled.";

                return RedirectToAction(
                    "MyBookings");
            }


            // Change booking status
            booking.Status = "Cancelled";


            // Return seat to the ride
            if (booking.Trip != null)
            {
                booking.Trip.SeatsAvailable++;
            }


            _db.SaveChanges();


            TempData["Success"] =
                "Booking cancelled successfully.";


            return RedirectToAction(
                "MyBookings");
        }
    }
}