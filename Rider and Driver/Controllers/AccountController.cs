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

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var username = model.Username.Trim().ToLower();
            var user = _db.Users.FirstOrDefault(u => u.Username.ToLower() == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ViewBag.Error = "Invalide Username or Password";
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            if (user.Role != "Rider" && user.Role != "Driver")
            {
                ModelState.AddModelError("Role", "Please select Rider or Driver");
                return View(user);
            }


            user.Username=user.Username.Trim();
            user.Email=user.Email.Trim();

            if (_db.Users.Any(u => u.Username.ToLower() == user.Username.ToLower()))
            {
                ViewBag.Error = "Username already taken";
                return View(user);
            }

            user.CreatedAt= DateTime.Now;

            user.Password= BCrypt.Net.BCrypt.HashPassword(user.Password);

            _db.Users.Add(user);
            _db.SaveChanges();

            ViewBag.Success = "Account created. Please log in";
            return RedirectToAction("Login");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
