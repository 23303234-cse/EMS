using EMS.Models;
using EMS.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EMS.Controllers
{
    public class PaymentController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public PaymentController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // ==========================
        // bKash Payment Page
        // ==========================
        public IActionResult BKash()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // ==========================
        // Nagad Payment Page
        // ==========================
        public IActionResult Nagad()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // ==========================
        // Card Payment Page
        // ==========================
        public IActionResult Card()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // ==========================
        // Payment Success
        // ==========================
        [HttpPost]
        public IActionResult Success()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = HttpContext.Session.GetString("UserEmail")!;

            string paymentMethod =
                HttpContext.Session.GetString("PaymentMethod")
                ?? "Cash on Delivery";

            var cartItems = _mongoDbService.Carts
                .Find(x => x.UserEmail == email)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Cart is empty.";

                return RedirectToAction("Index", "Cart");
            }

            Order order = new Order
            {
                UserEmail = email,

                OrderDate = DateTime.Now,

                Status = "Pending",

                PaymentMethod = paymentMethod,

                PaymentStatus = "Paid",

                TotalAmount = cartItems.Sum(x => x.Total)
            };

            foreach (var item in cartItems)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Image = item.Image
                });
            }

            _mongoDbService.Orders.InsertOne(order);

            _mongoDbService.Carts.DeleteMany(x => x.UserEmail == email);

            HttpContext.Session.Remove("PaymentMethod");

            TempData["Success"] =
                $"{paymentMethod} payment completed successfully.";

            return RedirectToAction("MyOrders", "Order");
        }
    }
}