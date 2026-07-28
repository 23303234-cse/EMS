using EMS.Models;
using EMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EMS.Controllers
{
    public class ProductController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public ProductController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // ==========================
        // Product List
        // ==========================
        public IActionResult Index(string search)
{
    if (HttpContext.Session.GetString("UserEmail") == null)
    {
        return RedirectToAction("Login", "Account");
    }

    // ✅ User হলে Home Dashboard-এ পাঠিয়ে দাও
    if (HttpContext.Session.GetString("UserRole") == "User")
    {
        return RedirectToAction("Dashboard", "Home");
    }

    // ✅ Admin হলে Product List দেখাও
    var products = string.IsNullOrEmpty(search)
        ? _mongoDbService.Products.Find(_ => true).ToList()
        : _mongoDbService.Products.Find(x => x.ProductName.Contains(search)).ToList();

    return View(products);
}

        // ==========================
        // Create Product (GET)
        // ==========================
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index");
            }

            ViewBag.Categories = _mongoDbService.Categories
                .Find(_ => true)
                .ToList();

            return View();
        }

        // ==========================
        // Create Product (POST)
        // ==========================
        [HttpPost]
        public IActionResult Create(Product product, IFormFile? imageFile)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index");
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() +
                                  Path.GetExtension(imageFile.FileName);

                string folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "products");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                product.Image = fileName;
            }

            _mongoDbService.Products.InsertOne(product);

            return RedirectToAction("Index");
        }

        // ==========================
        // Edit Product (GET)
        // ==========================
        public IActionResult Edit(string id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index");
            }

            var product = _mongoDbService.Products
                .Find(x => x.Id == id)
                .FirstOrDefault();

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _mongoDbService.Categories
                .Find(_ => true)
                .ToList();

            return View(product);
        }

        // ==========================
        // Edit Product (POST)
        // ==========================
        [HttpPost]
        public IActionResult Edit(Product product, IFormFile? imageFile)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index");
            }

            var existingProduct = _mongoDbService.Products
                .Find(x => x.Id == product.Id)
                .FirstOrDefault();

            if (existingProduct == null)
            {
                return NotFound();
            }

            // যদি নতুন Image Upload করা হয়
            if (imageFile != null && imageFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() +
                                  Path.GetExtension(imageFile.FileName);

                string folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "products");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                // পুরোনো Image Delete
                if (!string.IsNullOrEmpty(existingProduct.Image))
                {
                    string oldImage = Path.Combine(folderPath, existingProduct.Image);

                    if (System.IO.File.Exists(oldImage))
                    {
                        System.IO.File.Delete(oldImage);
                    }
                }

                product.Image = fileName;
            }
            else
            {
                // নতুন Image না দিলে পুরোনো Image রাখো
                product.Image = existingProduct.Image;
            }

            _mongoDbService.Products.ReplaceOne(x => x.Id == product.Id, product);

            return RedirectToAction("Index");
        }

        // ==========================
        // Delete Product
        // ==========================
        public IActionResult Delete(string id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Index");
            }

            var product = _mongoDbService.Products
                .Find(x => x.Id == id)
                .FirstOrDefault();

            if (product != null && !string.IsNullOrEmpty(product.Image))
            {
                string imagePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "products",
                    product.Image);

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _mongoDbService.Products.DeleteOne(x => x.Id == id);

            return RedirectToAction("Index");
        }
    }
}