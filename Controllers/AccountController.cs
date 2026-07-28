using EMS.Models;
using EMS.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public AccountController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // ==========================
        // Login (GET)
        // ==========================
        public IActionResult Login()
        {
            return View();
        }

        // ==========================
        // Register (GET)
        // ==========================
        public IActionResult Register()
        {
            return View();
        }

        // ==========================
        // Login (POST)
        // ==========================
        [HttpPost]
        public IActionResult Login(User user)
        {
            var existingUser = _mongoDbService.Users
                .Find(x => x.Email == user.Email && x.Password == user.Password)
                .FirstOrDefault();

            if (existingUser == null)
            {
                ViewBag.Message = "Invalid Email or Password";
                return View();
            }

            HttpContext.Session.SetString("UserEmail", existingUser.Email);
            HttpContext.Session.SetString("UserRole", existingUser.Role);
            HttpContext.Session.SetString("UserName", existingUser.Name);
            HttpContext.Session.SetString("UserProfileImage", existingUser.ProfileImage ?? "");

            return RedirectToAction("Dashboard", "Home");
        }

        // ==========================
        // Register (POST)
        // ==========================
        [HttpPost]
        public IActionResult Register(User user)
        {
            var existingUser = _mongoDbService.Users
                .Find(x => x.Email == user.Email)
                .FirstOrDefault();

            if (existingUser != null)
            {
                ViewBag.Message = "Email already exists!";
                return View();
            }

            user.Role = "User";

            // Default profile image
            user.ProfileImage = "";

            _mongoDbService.Users.InsertOne(user);

            TempData["Success"] = "Registration Successful! Please Login.";

            return RedirectToAction("Login");
        }

        // ==========================
        // Logout
        // ==========================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}