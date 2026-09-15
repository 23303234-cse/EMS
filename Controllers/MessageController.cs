using EMS.Models;
using EMS.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EMS.Controllers
{
    public class MessageController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public MessageController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // ==========================
        // User - Create Message (GET)
        // ==========================
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // ==========================
        // User - Send Message (POST)
        // ==========================
        [HttpPost]
        public IActionResult Create(Message message)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = HttpContext.Session.GetString("UserEmail")!;

            if (string.IsNullOrWhiteSpace(message.Subject))
            {
                ModelState.AddModelError(
                    "Subject",
                    "Subject is required.");
            }

            if (string.IsNullOrWhiteSpace(message.MessageText))
            {
                ModelState.AddModelError(
                    "MessageText",
                    "Message is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(message);
            }

            message.UserEmail = email;
            message.CreatedAt = DateTime.UtcNow;
            message.Reply = string.Empty;
            message.RepliedAt = null;
            message.Status = "Pending";

            _mongoDbService.Messages.InsertOne(message);

            TempData["Success"] =
                "Your message has been sent successfully.";

            return RedirectToAction("MyMessages");
        }

        // ==========================
        // User - My Messages
        // ==========================
        public IActionResult MyMessages()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = HttpContext.Session.GetString("UserEmail")!;

            var messages = _mongoDbService.Messages
                .Find(x => x.UserEmail == email)
                .SortByDescending(x => x.CreatedAt)
                .ToList();

            return View(messages);
        }

        // ==========================
        // User/Admin - Message Details
        // ==========================
        public IActionResult Details(string id)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var message = _mongoDbService.Messages
                .Find(x => x.Id == id)
                .FirstOrDefault();

            if (message == null)
            {
                return NotFound();
            }

            string? userRole =
                HttpContext.Session.GetString("UserRole");

            string? userEmail =
                HttpContext.Session.GetString("UserEmail");

            // User can only see their own message
            if (userRole != "Admin" &&
                message.UserEmail != userEmail)
            {
                return Forbid();
            }

            return View(message);
        }

        // ==========================
        // Admin - All Messages
        // ==========================
        public IActionResult AdminMessages()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var messages = _mongoDbService.Messages
                .Find(_ => true)
                .SortByDescending(x => x.CreatedAt)
                .ToList();

            return View(messages);
        }

        // ==========================
        // Admin - Reply (POST)
        // ==========================
        [HttpPost]
        public IActionResult Reply(string id, string reply)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(reply))
            {
                TempData["Error"] = "Reply cannot be empty.";
                return RedirectToAction("AdminMessages");
            }

            var update = Builders<Message>.Update
                .Set(x => x.Reply, reply.Trim())
                .Set(x => x.RepliedAt, DateTime.UtcNow)
                .Set(x => x.Status, "Replied");

            var result = _mongoDbService.Messages.UpdateOne(
                x => x.Id == id,
                update);

            if (result.MatchedCount == 0)
            {
                TempData["Error"] = "Message not found.";
                return RedirectToAction("AdminMessages");
            }

            TempData["Success"] =
                "Reply sent successfully.";

            return RedirectToAction("AdminMessages");
        }
    }
}