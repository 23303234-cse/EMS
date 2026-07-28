using EMS.Models;
using EMS.Services;
using MongoDB.Driver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMS.Controllers
{
    public class CategoryController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public CategoryController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // ===========================
        // Category List
        // ===========================
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var categories = _mongoDbService.Categories
                .Find(_ => true)
                .ToList();

            return View(categories);
        }

        // ===========================
        // Create Category (GET)
        // ===========================
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        // ===========================
        // Create Category (POST)
        // ===========================
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index");
            }

            _mongoDbService.Categories.InsertOne(category);

            return RedirectToAction("Index");
        }

        // ===========================
        // Edit Category (GET)
        // ===========================
        public IActionResult Edit(string id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index");
            }

            var category = _mongoDbService.Categories
                .Find(x => x.Id == id)
                .FirstOrDefault();

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // ===========================
        // Edit Category (POST)
        // ===========================
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index");
            }

            _mongoDbService.Categories.ReplaceOne(x => x.Id == category.Id, category);

            return RedirectToAction("Index");
        }

        // ===========================
        // Delete Category
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
            public IActionResult Delete(string id)
            {
                if (HttpContext.Session.GetString("UserRole") != "Admin")
                {
                    return RedirectToAction("Index");
                }

                    _mongoDbService.Categories.DeleteOne(x => x.Id == id);

                    return RedirectToAction("Index");
            }
    }
}