using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rider_and_Driver.Data;
using Rider_and_Driver.Models;

namespace Rider_and_Driver.Controllers
{
    [Authorize]
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

        //handles the creation of new bookings
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Book(int tripId)
        {
            //authenication and authorisation checks
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

            //Ensure only usres with rider can book
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

            // trip validatiom
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

            //Ensures the rider hasnt all ready book a seat on that ride
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

            //Creates new booking
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

            //Reduces avaliblity to prevent overbooking
            trip.SeatsAvailable--;

            //Save Transaction to database
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

        //Displays a list of made by the current looged in driver
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

            // Queries the database for booking belongng to rider
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

        //allows a rider to cancel their booking 
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

            //A rider cannot cancel some elses booking by guessing ID
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

            //Changes booking status
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