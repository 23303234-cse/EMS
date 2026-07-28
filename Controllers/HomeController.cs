using System.Diagnostics;
using EMS.Models;
using EMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public HomeController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // ==========================
        // Home
        // ==========================
        public IActionResult Index()
        {
            return View();
        }

        // ==========================
        // Dashboard
        // ==========================
        public IActionResult Dashboard(string? search)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string? role = HttpContext.Session.GetString("UserRole");

            // ==========================
            // USER DASHBOARD
            // ==========================
            if (role == "User")
            {
                var products = string.IsNullOrWhiteSpace(search)
                    ? _mongoDbService.Products
                        .Find(_ => true)
                        //.Limit(8)
                        .ToList()
                    : _mongoDbService.Products
                        .Find(x =>
                            x.ProductName.ToLower().Contains(search.ToLower()) ||
                            x.Category.ToLower().Contains(search.ToLower()))
                        .ToList();

                return View("UserDashboard", products);
            }

            // ==========================
            // ADMIN DASHBOARD
            // ==========================

            ViewBag.TotalProducts = _mongoDbService.Products.CountDocuments(_ => true);

            ViewBag.TotalUsers = _mongoDbService.Users.CountDocuments(_ => true);

            ViewBag.TotalCategories = _mongoDbService.Products
                .Distinct<string>("Category", FilterDefinition<Product>.Empty)
                .ToList()
                .Count;

            ViewBag.LowStock = _mongoDbService.Products
                .CountDocuments(x => x.Stock <= 5);

            ViewBag.TotalOrders = _mongoDbService.Orders
                .CountDocuments(_ => true);

            decimal totalRevenue = 0;

            var orders = _mongoDbService.Orders
                .Find(_ => true)
                .ToList();

            foreach (var order in orders)
            {
                totalRevenue += order.TotalAmount;
            }

            ViewBag.TotalRevenue = totalRevenue;

            ViewBag.Months = new string[]
            {
                "Jan",
                "Feb",
                "Mar",
                "Apr",
                "May",
                "Jun"
            };

            ViewBag.Sales = new int[]
            {
                12,
                19,
                8,
                15,
                25,
                30
            };

            return View();
        }

        // ==========================
        // Privacy
        // ==========================
        public IActionResult Privacy()
        {
            return View();
        }

        // ==========================
        // Error
        // ==========================
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}