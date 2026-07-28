using EMS.Models;
using EMS.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.IO;

namespace EMS.Controllers
{
    public class ProfileController : Controller
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(MongoDbService mongoDbService, IWebHostEnvironment webHostEnvironment)
        {
            _mongoDbService = mongoDbService;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================
        // My Profile
        // ==========================
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = HttpContext.Session.GetString("UserEmail")!;

            var user = _mongoDbService.Users
                .Find(x => x.Email == email)
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // ==========================
        // Edit Profile (GET)
        // ==========================
        public IActionResult Edit()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = HttpContext.Session.GetString("UserEmail")!;

            var user = _mongoDbService.Users
                .Find(x => x.Email == email)
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // ==========================
        // Edit Profile (POST)
        // ==========================
        [HttpPost]
        public IActionResult Edit(User model, IFormFile? profileImage)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var existingUser = _mongoDbService.Users
                .Find(x => x.Id == model.Id)
                .FirstOrDefault();

            if (existingUser == null)
            {
                return NotFound();
            }

            // Update basic information
            existingUser.Name = model.Name;
            existingUser.Phone = model.Phone;
            existingUser.Address = model.Address;

            // Upload profile picture
            if (profileImage != null && profileImage.Length > 0)
            {
                var extension = Path.GetExtension(profileImage.FileName).ToLower();

                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Only JPG, JPEG and PNG files are allowed.";
                    return View(model);
                }

                string folderPath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "images",
                    "profiles");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Delete old image
                if (!string.IsNullOrEmpty(existingUser.ProfileImage))
                {
                    string oldImagePath = Path.Combine(folderPath, existingUser.ProfileImage);

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                string fileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    profileImage.CopyTo(stream);
                }

                existingUser.ProfileImage = fileName;
            }

            _mongoDbService.Users.ReplaceOne(x => x.Id == existingUser.Id, existingUser);

            // Update Session
            HttpContext.Session.SetString("UserName", existingUser.Name);
            HttpContext.Session.SetString("UserProfileImage", existingUser.ProfileImage ?? "");

            TempData["Success"] = "Profile updated successfully.";

            return RedirectToAction("Index");
        }

        // ==========================
        // Change Password (GET)
        // ==========================
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // ==========================
        // Change Password (POST)
        // ==========================
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = HttpContext.Session.GetString("UserEmail")!;

            var user = _mongoDbService.Users
                .Find(x => x.Email == email)
                .FirstOrDefault();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (user.Password != model.OldPassword)
            {
                ViewBag.Message = "Old password is incorrect.";
                return View(model);
            }

            user.Password = model.NewPassword;

            _mongoDbService.Users.ReplaceOne(x => x.Id == user.Id, user);

            TempData["Success"] = "Password changed successfully.";

            return RedirectToAction("Index");
        }
    }
}