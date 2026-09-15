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
    // Name validation
    if (string.IsNullOrWhiteSpace(user.Name))
    {
        ModelState.AddModelError("Name", "Name is required.");
    }
    else if (user.Name.Trim().Length < 2)
    {
        ModelState.AddModelError("Name", "Name must be at least 2 characters.");
    }

    // Phone validation
    if (string.IsNullOrWhiteSpace(user.Phone))
    {
        ModelState.AddModelError("Phone", "Phone number is required.");
    }
    else if (!System.Text.RegularExpressions.Regex.IsMatch(
        user.Phone.Trim(), @"^01[3-9]\d{8}$"))
    {
        ModelState.AddModelError(
            "Phone",
            "Enter a valid Bangladesh phone number (e.g. 01712345678).");
    }

    // Email validation
    if (string.IsNullOrWhiteSpace(user.Email))
    {
        ModelState.AddModelError("Email", "Email is required.");
    }
    else
    {
        var emailValidator =
    new System.ComponentModel.DataAnnotations.EmailAddressAttribute();

    if (!emailValidator.IsValid(user.Email))
    {
    ModelState.AddModelError("Email", "Enter a valid email address.");
    }
    else if (user.Email.Any(char.IsUpper))
    {
        ModelState.AddModelError(
        "Email",
        "Email must be entered in lowercase letters only.");
    }
    }

    // Password validation
    if (string.IsNullOrWhiteSpace(user.Password))
    {
        ModelState.AddModelError("Password", "Password is required.");
    }
    else if (user.Password.Length < 6)
    {
        ModelState.AddModelError(
            "Password",
            "Password must be at least 6 characters.");
    }
    // Confirm Password validation
if (string.IsNullOrWhiteSpace(user.ConfirmPassword))
{
    ModelState.AddModelError(
        "ConfirmPassword",
        "Confirm Password is required.");
}

else if (user.Password != user.ConfirmPassword)
{
    ModelState.AddModelError(
        "ConfirmPassword",
        "Passwords do not match.");
}

    // Stop if validation fails
    if (!ModelState.IsValid)
    {
        return View(user);
    }

    // Check duplicate email
    var existingEmail = _mongoDbService.Users
        .Find(x => x.Email == user.Email)
        .FirstOrDefault();

    if (existingEmail != null)
    {
        ModelState.AddModelError("Email", "Email already exists!");
        return View(user);
    }

    // Check duplicate phone
    var existingPhone = _mongoDbService.Users
        .Find(x => x.Phone == user.Phone)
        .FirstOrDefault();

    if (existingPhone != null)
    {
        ModelState.AddModelError("Phone", "Phone number already exists!");
        return View(user);
    }

    // Default role
    user.Role = "User";

    // Default profile image
    user.ProfileImage = "";

    // Save user
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