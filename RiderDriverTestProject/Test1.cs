
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Rider_and_Driver.Controllers;
using Rider_and_Driver.Data;
using Rider_and_Driver.Models;


namespace Rider_and_Driver.Tests
{
    [TestClass]
    public class UnitTest1
    {
        // Create an in-memory database for testing
        private ApplicationDbContext GetDbContext()
        {
            // Create a temporary database for each test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            // Add a test driver and rider
            context.Users.AddRange(
                new User
                {
                    UserId = 1,
                    Username = "testdriver",
                    Email = "testdriver@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("12345678"),
                    Role = "Driver",
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    UserId = 2,
                    Username = "testrider",
                    Email = "testrider@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("12345678"),
                    Role = "Rider",
                    CreatedAt = DateTime.Now
                }
            );

            // Save the test data
            context.SaveChanges();

            return context;
        }

        [TestMethod]
        // Test registering a rider
        public void Register_Rider_CreatesAccount()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new AccountController(context);

            var newRider = new User
            {
                Username = "newtestrider",
                Email = "newtestrider@example.com",
                Password = "12345678",
                Role = "Rider",
                CreatedAt = DateTime.Now
            };

            // Act
            IActionResult result = controller.Register(newRider);

            // Assert
            var rider = context.Users
                .FirstOrDefault(u => u.Username == "newtestrider");

            Assert.IsNotNull(rider);
            Assert.AreEqual("newtestrider", rider.Username);
            Assert.AreEqual("newtestrider@example.com", rider.Email);
            Assert.AreEqual("Rider", rider.Role);

            // Check that the password is hashed
            Assert.IsTrue(
                BCrypt.Net.BCrypt.Verify("12345678", rider.Password));

            // Check that the rider record was created
            var riderRecord = context.Riders
                .FirstOrDefault(r => r.UserId == rider.UserId);

            Assert.IsNotNull(riderRecord);

            // Check that registration redirects to Login
            Assert.IsInstanceOfType(
                result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Login", redirectResult.ActionName);
        }

        [TestMethod]
        // Test registering a driver
        public void Register_Driver_CreatesAccount()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new AccountController(context);

            var newDriver = new User
            {
                Username = "newtestdriver",
                Email = "newtestdriver@example.com",
                Password = "12345678",
                Role = "Driver",
                CreatedAt = DateTime.Now
            };

            // Act
            IActionResult result = controller.Register(newDriver);

            // Assert
            var driver = context.Users
                .FirstOrDefault(u => u.Username == "newtestdriver");

            Assert.IsNotNull(driver);
            Assert.AreEqual("newtestdriver", driver.Username);
            Assert.AreEqual("newtestdriver@example.com", driver.Email);
            Assert.AreEqual("Driver", driver.Role);

            // Check that the password is hashed
            Assert.IsTrue(
                BCrypt.Net.BCrypt.Verify("12345678", driver.Password));

            // Check that the driver record was created
            var driverRecord = context.Drivers
                .FirstOrDefault(d => d.UserId == driver.UserId);

            Assert.IsNotNull(driverRecord);

            // Check that registration redirects to Login
            Assert.IsInstanceOfType(
                result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Login", redirectResult.ActionName);
        }

        [TestMethod]
        // Test logging in as a rider
        public void Login_Rider_RedirectsToRiderHomepage()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new AccountController(context);

            // Set up the HTTP context and session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var loginDetails = new LoginViewModel
            {
                Username = "testrider",
                Password = "12345678"
            };

            // Act
            IActionResult result = controller.Login(loginDetails);

            // Assert
            Assert.IsInstanceOfType(
                result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;

            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("RiderOnly", redirectResult.ControllerName);
        }

        [TestMethod]
        // Test logging in as a driver
        public void Login_Driver_RedirectsToDriverHomepage()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new AccountController(context);

            // Set up the HTTP context and session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var loginDetails = new LoginViewModel
            {
                Username = "testdriver",
                Password = "12345678"
            };

            // Act
            IActionResult result = controller.Login(loginDetails);

            // Assert
            Assert.IsInstanceOfType(
                result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;

            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("DriverOnly", redirectResult.ControllerName);
        }

        [TestMethod]
        // Test creating a trip with valid details
        public void CreateTrip_ValidDetails_SavesTrip()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new TripController(context);

            // Set up the HTTP context and session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Set up TempData for the controller
            controller.TempData = new TempDataDictionary(
                httpContext, new TestTempDataProvider());

            // Set the logged-in driver's UserId
            httpContext.Session.SetInt32("UserId", 1);

            var newTrip = new Trip
            {
                DepartureDate = DateTime.Today.AddDays(2),
                DepartureTime = new TimeSpan(10, 0, 0),
                FromLocation = "Lower Hutt",
                ToLocation = "Wellington",
                SeatsAvailable = 4,
                Cost = 12.50m,
                NumberPlate = "ABC123",
                Status = "Active"
            };

            // Act
            IActionResult result = controller.Create(newTrip);

            // Assert
            var trip = context.Trips
                .FirstOrDefault(t => t.FromLocation == "Lower Hutt"
                                  && t.ToLocation == "Wellington");

            Assert.IsNotNull(trip);
            Assert.AreEqual("Lower Hutt", trip.FromLocation);
            Assert.AreEqual("Wellington", trip.ToLocation);
            Assert.AreEqual(4, trip.SeatsAvailable);
            Assert.AreEqual(12.50m, trip.Cost);

            // Check that Create redirects to Index
            Assert.IsInstanceOfType(
                result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }



        // Testing the Driver Edit Trip Method
        [TestMethod]
        public void EditTrip_ValidDetails_UpdatesTrip()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new TripController(context);

            // Set up the HTTP context and session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Set up TempData for the controller
            controller.TempData = new TempDataDictionary(
                httpContext, new TestTempDataProvider());

            // Set the logged-in driver's UserId
            httpContext.Session.SetInt32("UserId", 1);

            // Create an existing trip
            var existingTrip = new Trip
            {
                DriverUserId = 1,
                DepartureDate = DateTime.Today.AddDays(2),
                DepartureTime = new TimeSpan(10, 0, 0),
                FromLocation = "Lower Hutt",
                ToLocation = "Wellington",
                SeatsAvailable = 4,
                Cost = 12.50m,
                NumberPlate = "ABC123",
                Status = "Active"
            };

            // Save the trip in the database
            context.Trips.Add(existingTrip);
            context.SaveChanges();

            // Create updated trip details
            var updatedTrip = new Trip
            {
                TripId = existingTrip.TripId,
                DriverUserId = 1,
                DepartureDate = DateTime.Today.AddDays(3),
                DepartureTime = new TimeSpan(11, 0, 0),
                FromLocation = "Lower Hutt",
                ToLocation = "Porirua",
                SeatsAvailable = 3,
                Cost = 15.50m,
                NumberPlate = "XYZ789",
                Status = "Active"
            };

            // Act
            // Update the existing trip
            IActionResult result = controller.Edit(
                existingTrip.TripId, updatedTrip);

            // Assert
            // Check that the trip has been updated
            var savedTrip = context.Trips
                .AsNoTracking()
                .FirstOrDefault(t =>
                    t.TripId == existingTrip.TripId);

            Assert.IsNotNull(savedTrip);

            // Check the updated trip details
            Assert.AreEqual("Lower Hutt", savedTrip.FromLocation);
            Assert.AreEqual("Porirua", savedTrip.ToLocation);
            Assert.AreEqual(DateTime.Today.AddDays(3), savedTrip.DepartureDate.Date);
            Assert.AreEqual(new TimeSpan(11, 0, 0), savedTrip.DepartureTime);
            Assert.AreEqual(3, savedTrip.SeatsAvailable);
            Assert.AreEqual(15.50m, savedTrip.Cost);
            Assert.AreEqual("XYZ789", savedTrip.NumberPlate);

            // Check that the update redirects to Details
            Assert.IsInstanceOfType(
                result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;

            Assert.AreEqual("Details", redirectResult.ActionName);

            // Check that the correct trip ID is passed
            Assert.IsNotNull(redirectResult.RouteValues);
            Assert.AreEqual(
                existingTrip.TripId,
                redirectResult.RouteValues["id"]);
        }
        // Testing the Driver Cancel Trip Method
        [TestMethod]
        public void Driver_CancelTrip_UpdatesTripStatus()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new TripController(context);

            // Set up the HTTP context and session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Set up TempData for the controller
            controller.TempData = new TempDataDictionary(
                httpContext, new TestTempDataProvider());

            // Set the logged-in driver's UserId
            httpContext.Session.SetInt32("UserId", 1);

            // Create an existing trip
            var existingTrip = new Trip
            {
                DriverUserId = 1,
                DepartureDate = DateTime.Today.AddDays(2),
                DepartureTime = new TimeSpan(10, 0, 0),
                FromLocation = "Lower Hutt",
                ToLocation = "Wellington",
                SeatsAvailable = 4,
                Cost = 12.50m,
                NumberPlate = "ABC123",
                Status = "Active"
            };

            // Save the trip in the database
            context.Trips.Add(existingTrip);
            context.SaveChanges();

            // Act
            // Cancel the driver's trip
            IActionResult result = controller.Cancel(existingTrip.TripId);

            // Assert
            // Check that the trip still exists
            var cancelledTrip = context.Trips
                .AsNoTracking()
                .FirstOrDefault(t =>
                    t.TripId == existingTrip.TripId);

            Assert.IsNotNull(cancelledTrip);

            // Check that the trip status is Cancelled
            Assert.AreEqual("Cancelled", cancelledTrip.Status);

            // Check that cancellation redirects to Index
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Index", redirectResult.ActionName);
        }
        // Testing the Rider Cancel Booking Method
        [TestMethod]
        public void Rider_CancelBooking_UpdatesBookingStatusAndReturnsSeat()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new BookingController(context);

            // Set up the HTTP context and session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Set up TempData for the controller
            controller.TempData = new TempDataDictionary(
                httpContext, new TestTempDataProvider());

            // Set the logged-in rider's UserId
            httpContext.Session.SetInt32("UserId", 2);

            // Create an existing trip
            var existingTrip = new Trip
            {
                DriverUserId = 1,
                DepartureDate = DateTime.Today.AddDays(2),
                DepartureTime = new TimeSpan(10, 0, 0),
                FromLocation = "Lower Hutt",
                ToLocation = "Wellington",
                SeatsAvailable = 3,
                Cost = 12.50m,
                NumberPlate = "ABC123",
                Status = "Active"
            };

            // Save the trip in the database
            context.Trips.Add(existingTrip);
            context.SaveChanges();

            // Create an existing booking for the rider
            var existingBooking = new Booking
            {
                TripId = existingTrip.TripId,
                RiderUserId = 2,
                TotalCost = 12.50m,
                Status = "Confirmed",
                BookedAt = DateTime.Now
            };

            // Save the booking in the database
            context.Bookings.Add(existingBooking);
            context.SaveChanges();

            // Act
            // Cancel the rider's booking
            IActionResult result = controller.Cancel(existingBooking.BookingId);

            // Assert
            // Check that the booking still exists
            var cancelledBooking = context.Bookings
                .AsNoTracking()
                .FirstOrDefault(b =>
                    b.BookingId == existingBooking.BookingId);

            Assert.IsNotNull(cancelledBooking);

            // Check that the booking status is Cancelled
            Assert.AreEqual("Cancelled", cancelledBooking.Status);

            // Check that the seat has been returned to the ride
            var updatedTrip = context.Trips
                .AsNoTracking()
                .FirstOrDefault(t =>
                    t.TripId == existingTrip.TripId);

            Assert.IsNotNull(updatedTrip);
            Assert.AreEqual(4, updatedTrip.SeatsAvailable);

            // Check that cancellation redirects to MyBookings
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("MyBookings", redirectResult.ActionName);
        }
        // Testing the Search Rides Method
        [TestMethod]
        public void SearchRides_ByDepartureLocation_ReturnsMatchingTrips()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new TripController(context);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetInt32("UserId", 1);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Create the first trip
            var trip1 = new Trip
            {
                DriverUserId = 1,
                DepartureDate = DateTime.Today.AddDays(2),
                DepartureTime = new TimeSpan(10, 0, 0),
                FromLocation = "Lower Hutt",
                ToLocation = "Wellington",
                SeatsAvailable = 4,
                Cost = 12.50m,
                NumberPlate = "ABC123",
                Status = "Active"
            };

            // Create the second trip
            var trip2 = new Trip
            {
                DriverUserId = 1,
                DepartureDate = DateTime.Today.AddDays(3),
                DepartureTime = new TimeSpan(11, 0, 0),
                FromLocation = "Porirua",
                ToLocation = "Wellington",
                SeatsAvailable = 3,
                Cost = 15.00m,
                NumberPlate = "XYZ789",
                Status = "Active"
            };

            context.Trips.AddRange(trip1, trip2);
            context.SaveChanges();

            // Act
            // Search for trips departing from Lower Hutt
            IActionResult result = controller.Index(
                FromLocation: "Lower Hutt",
                ToLocation: null,
                DepartureDate: null,
                SortBy: null,
                SortOrder: null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            var viewResult = (ViewResult)result;
            var trips = ((IEnumerable<Trip>)viewResult.Model!).ToList();

            // Check that only the matching trip is returned
            Assert.AreEqual(1, trips.Count);
            Assert.AreEqual("Lower Hutt", trips[0].FromLocation);
        }
        // Testing the Sort Rides by Cost Method
        [TestMethod]
        public void SortRides_ByCostAscending_ReturnsTripsInCostOrder()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new TripController(context);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetInt32("UserId", 1);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Create a more expensive trip
            var trip1 = new Trip
            {
                DriverUserId = 1,
                DepartureDate = DateTime.Today.AddDays(2),
                DepartureTime = new TimeSpan(10, 0, 0),
                FromLocation = "Lower Hutt",
                ToLocation = "Wellington",
                SeatsAvailable = 4,
                Cost = 20.00m,
                NumberPlate = "ABC123",
                Status = "Active"
            };

            // Create a cheaper trip
            var trip2 = new Trip
            {
                DriverUserId = 1,
                DepartureDate = DateTime.Today.AddDays(3),
                DepartureTime = new TimeSpan(11, 0, 0),
                FromLocation = "Porirua",
                ToLocation = "Wellington",
                SeatsAvailable = 3,
                Cost = 10.00m,
                NumberPlate = "XYZ789",
                Status = "Active"
            };

            // Save the trips
            context.Trips.AddRange(trip1, trip2);
            context.SaveChanges();

            // Act
            // Sort trips by cost from lowest to highest
            IActionResult result = controller.Index(
                FromLocation: null,
                ToLocation: null,
                DepartureDate: null,
                SortBy: "cost",
                SortOrder: "asc");

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            var viewResult = (ViewResult)result;
            var trips = ((IEnumerable<Trip>)viewResult.Model!).ToList();

            // Check that both trips are returned
            Assert.AreEqual(2, trips.Count);

            // Check that the cheaper trip appears first
            Assert.AreEqual(10.00m, trips[0].Cost);
            Assert.AreEqual(20.00m, trips[1].Cost);
        }
        // Testing the Rider Book Ride Method
        [TestMethod]
        public void BookRide_ValidDetails_SavesBooking()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new BookingController(context);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetString("Username", "testrider");
            httpContext.Session.SetString("Role", "Rider");
            httpContext.Session.SetInt32("UserId", 2);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(
                httpContext, new TestTempDataProvider());

            // Create an available trip
            var trip = new Trip
            {
                DriverUserId = 1,
                DepartureDate = DateTime.Today.AddDays(2),
                DepartureTime = new TimeSpan(10, 0, 0),
                FromLocation = "Lower Hutt",
                ToLocation = "Wellington",
                SeatsAvailable = 4,
                Cost = 12.50m,
                NumberPlate = "ABC123",
                Status = "Active"
            };

            context.Trips.Add(trip);
            context.SaveChanges();

            // Act
            // Book the available trip
            IActionResult result = controller.Book(trip.TripId);

            // Assert
            // Check that the booking was saved
            var savedBooking = context.Bookings
                .AsNoTracking()
                .FirstOrDefault(b =>
                    b.TripId == trip.TripId &&
                    b.RiderUserId == 2);

            Assert.IsNotNull(savedBooking);

            // Check the booking details
            Assert.AreEqual("Confirmed", savedBooking.Status);
            Assert.AreEqual(12.50m, savedBooking.TotalCost);

            // Check that one seat has been taken
            var updatedTrip = context.Trips
                .AsNoTracking()
                .FirstOrDefault(t => t.TripId == trip.TripId);

            Assert.IsNotNull(updatedTrip);
            Assert.AreEqual(3, updatedTrip.SeatsAvailable);

            // Check the redirect to MyBookings
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("MyBookings", redirectResult.ActionName);
        }
        // Testing the Driver View Trip Details Method
        [TestMethod]
        public void ViewTripDetails_ValidTrip_ReturnsTripDetails()
        {
            // Arrange
            using var context = GetDbContext();
            var controller = new TripController(context);

            // Set up the HTTP context and session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetInt32("UserId", 1);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Set up TempData
            controller.TempData = new TempDataDictionary(
                httpContext, new TestTempDataProvider());

            // Create an existing trip
            var existingTrip = new Trip
            {
                DriverUserId = 1,
                DepartureDate = DateTime.Today.AddDays(2),
                DepartureTime = new TimeSpan(10, 0, 0),
                FromLocation = "Lower Hutt",
                ToLocation = "Wellington",
                SeatsAvailable = 4,
                Cost = 12.50m,
                NumberPlate = "ABC123",
                Status = "Active"
            };

            // Save the trip in the database
            context.Trips.Add(existingTrip);
            context.SaveChanges();

            // Act
            // Open the trip details
            IActionResult result = controller.Details(existingTrip.TripId);

            // Assert
            // Check that the details page is returned
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            var viewResult = (ViewResult)result;

            // Check that the correct trip is displayed
            Assert.IsNotNull(viewResult.Model);

            var displayedTrip = (Trip)viewResult.Model;

            Assert.AreEqual(existingTrip.TripId, displayedTrip.TripId);
            Assert.AreEqual("Lower Hutt", displayedTrip.FromLocation);
            Assert.AreEqual("Wellington", displayedTrip.ToLocation);
            Assert.AreEqual(12.50m, displayedTrip.Cost);
            Assert.AreEqual("ABC123", displayedTrip.NumberPlate);
        }

        // Helper class to simulate a user session
        private class TestSession : ISession
        {
            // Store session data during the test
            private readonly Dictionary<string, byte[]> _data = new();

            // Get the session keys, ID, and availability
            public IEnumerable<string> Keys => _data.Keys;
            public string Id => "TestSession";
            public bool IsAvailable => true;

            // Remove all session data
            public void Clear()
            {
                _data.Clear();
            }

            // Remove a specific session item
            public void Remove(string key)
            {
                _data.Remove(key);
            }

            // Save a value in the session
            public void Set(string key, byte[] value)
            {
                _data[key] = value;
            }

            // Retrieve a value from the session
            public bool TryGetValue(string key, out byte[] value)
            {
                return _data.TryGetValue(key, out value);
            }

            // Required by .NET 8 to load session data
            public Task LoadAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            // Required by .NET 8 to save session data
            public Task CommitAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        // Helper class to simulate TempData in unit tests
        private class TestTempDataProvider : ITempDataProvider
        {
            // Load temporary data for the controller
            public IDictionary<string, object> LoadTempData(
                HttpContext context)
            {
                // Return an empty collection for the test
                return new Dictionary<string, object>();
            }

            // Save temporary data created by the controller
            public void SaveTempData(
                HttpContext context,
                IDictionary<string, object> values)
            {
                // No saving is required for this unit test
            }
        }
    }
}
