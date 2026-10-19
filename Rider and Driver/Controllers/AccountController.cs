using Microsoft.AspNetCore.Mvc;
using Rider_and_Driver.Data;
using Rider_and_Driver.Models;
using Microsoft.AspNetCore.Http;

namespace Rider_and_Driver.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        //Displays Login form. If the user is alreadt authenicated
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        //Handles authentication. Uses BCrypt. Verify to compare the submitted password agianst the hashed password in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            //Normalise inputs
            var username = model.Username.Trim().ToLower();
            var user = _db.Users.FirstOrDefault(u => u.Username.ToLower() == username);

            //Verifies the user exists an the password matches
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ViewBag.Error = "Invalide Username or Password";
                return View(model);
            }

            //sets up the session for authenticatited state
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            //Roled based routing
            if (user.Role == "Rider")
            {
                return RedirectToAction("Index", "RiderOnly");
            }

            if (user.Role == "Driver")
            {
                return RedirectToAction("Index", "DriverOnly");
            }

            //Fallback redirect if role not reconised
            return RedirectToAction("Index", "Home");
        }
        
        //Displays the regististration form, if user is already loged in they are redirected away
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }


        //Handles user creatation, password hashing and database relationship setup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            //Server side validation
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            //Ensure the role is strictly either rider or driver
            if (user.Role != "Rider" && user.Role != "Driver")
            {
                ModelState.AddModelError("Role", "Please select Rider or Driver");
                return View(user);
            }

            //Normalise inputs
            user.Username=user.Username.Trim();
            user.Email=user.Email.Trim();

            //Checks for duplicate username
            if (_db.Users.Any(u => u.Username.ToLower() == user.Username.ToLower()))
            {
                ViewBag.Error = "Username already taken";
                return View(user);
            }

            user.CreatedAt= DateTime.Now;

            user.Password= BCrypt.Net.BCrypt.HashPassword(user.Password);
            // Save user first
            _db.Users.Add(user);
            _db.SaveChanges();

            // Create rider or drvier record
            if (user.Role == "Rider")
            {
                var rider = new Rider
                {
                    UserId = user.UserId
                };

                _db.Riders.Add(rider);
            }
            else if (user.Role == "Driver")
            {
                var driver = new Driver
                {
                    UserId = user.UserId,
                    LicenseNumber = "Not Provided"
                };

                _db.Drivers.Add(driver);
            }

            //Save the rider or driver record to th database
            _db.SaveChanges();

            ViewBag.Success = "Account created. Please log in";
            return RedirectToAction("Login");

        }

        //Clears the users session data and redirects to the login page.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
