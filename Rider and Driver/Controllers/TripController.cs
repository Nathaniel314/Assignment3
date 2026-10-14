using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rider_and_Driver.Data;
using Rider_and_Driver.Models;

namespace Rider_and_Driver.Controllers
{
    public class TripController : DriverOnlyController
    {
        private readonly ApplicationDbContext _db;

        public TripController(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        private static readonly List<string> Locations = new()
        {
            "Lower Hutt",
            "Upper Hutt",
            "Wellington",
            "Petone",
            "Porirua",
            "Paraparaumu"
        };

        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Trip trip)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (trip.FromLocation == trip.ToLocation)
            {
                ModelState.AddModelError("ToLocation", "Destination must be different from Departure location");
            }

            if (trip.DepartureDate < DateTime.Now)
            {
                ModelState.AddModelError("DepartureDate", "Departure Date cannot be in the past (We don't drive DeLoreans)");
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(trip);
            }

            trip.DriverUserId = userId.Value;
            trip.CreatedAt = DateTime.Now;

            _db.Trips.Add(trip);
            _db.SaveChanges();

            TempData["Success"] = "Trip created successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Index(
            string? FromLocation,
            string? ToLocation,
            DateTime? DepartureDate,
            string? SortBy,
            string? SortOrder )
        {
            var driverId = HttpContext.Session.GetInt32("UserId");

            if (driverId == null)
                return RedirectToAction("login", "Account");

            var trips = _db.Trips
                .Where(t => t.DriverUserId == driverId && t.Status == "Active");
                
                //This was the default order and sort that the application already does by Mehakpreet code
                //.OrderBy(t => t.DepartureDate)
                //.ThenBy(t => t.DepartureTime)
                //.ToList();

            //Searching Departure Location code
            if (!string.IsNullOrWhiteSpace(FromLocation))
            {
                trips = trips.Where(t => t.FromLocation.Contains(FromLocation));
            }

            //Search Destination
            if (!string.IsNullOrWhiteSpace(ToLocation))
            {
                trips = trips.Where(t => t.ToLocation.Contains(ToLocation));
            }

            //Search by Departyre Date 
            if (DepartureDate.HasValue)
            {
                trips = trips.Where(t => t.DepartureDate.Date == DepartureDate.Value.Date);
            }

            //Sorting
            //Sort Cost
            if (SortBy == "cost")
            {
                if (SortOrder == "desc")
                    trips = trips.OrderByDescending (t => t.Cost);
                else
                    trips = trips.OrderBy (t => t.Cost);
            }
            //Sort Date
            else if (SortBy == "date")
            {
                if (SortOrder == "desc")
                    trips = trips.OrderByDescending(t => t.DepartureDate)
                        .ThenByDescending(t => t.DepartureTime);
                else
                    trips = trips.OrderBy(t => t.DepartureDate)
                        .ThenBy(t => t.DepartureTime);
            }
            //Sort Seats
            else if (SortBy == "seats")
            {
                if (SortOrder == "desc")
                    trips = trips.OrderByDescending(t => t.SeatsAvailable);
                else
                    trips = trips.OrderBy(t => t.SeatsAvailable);
            }
            //Default Sort
            else
            {
                trips = trips
                    .OrderBy(t => t.DepartureDate)
                    .ThenBy(t => t.DepartureTime);
            }

            trips.ToList();


            var bookingCounts= _db.Bookings
                .Where(b => b.Status == "Confirmed")
                .GroupBy(b => b.TripId)
                .Select(g => new {TripId= g.Key, Count=g.Count()})
                .ToDictionary(x=> x.TripId, x => x.Count);

            ViewBag.BookingCounts= bookingCounts;
            return View(trips);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var driverId = HttpContext.Session.GetInt32("UserId");
            if (driverId == null)
                return RedirectToAction("Login", "Account");

            var trip = _db.Trips
                .Include(t => t.Driver)
                .FirstOrDefault(t => t.TripId == id);

            if (trip == null)
                return NotFound();

            if (trip.DriverUserId != driverId)
            {
                TempData["Error"] = "You can only view your own trips.";
                return RedirectToAction("Index");
            }

            var bookings = _db.Bookings
                .Include(b=> b.Rider)
                .Where(b => b.TripId==id)
                .OrderByDescending(b => b.BookedAt)
                .ToList();

            ViewBag.Bookings= bookings;
            ViewBag.ConfirmedCount = bookings.Count(b => b.Status=="Confirmed");

            return View(trip);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var driverId = HttpContext.Session.GetInt32("UserId");
            if (driverId == null)
                return RedirectToAction("Login", "Account");

            var trip = _db.Trips.FirstOrDefault(t=> t.TripId==id);

            if(trip == null)
                return NotFound();

            if (trip.DriverUserId != driverId)
            {
                TempData["Error"] = "You can only cancel your own trips.";
                return RedirectToAction("Index");
            }

            trip.Status = "Cancelled";

            var activeBookings = _db.Bookings
                .Where(b => b.TripId == id && b.Status == "Confirmed")
                .ToList();

            foreach (var booking in activeBookings)
            {
                booking.Status = "Cancelled";
            }

            _db.SaveChanges();

            TempData["Success"] = $"Trip cancelled ({activeBookings.Count} booking(s) cancelled)";
            return RedirectToAction("Index");
        }

        private void LoadDropdowns()
        {
            ViewBag.Locations=new SelectList(Locations);
        }
    }
}
