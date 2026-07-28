using EMS.Models;
using EMS.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EMS.Controllers
{
    public class OrderController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public OrderController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // ==========================
        // Checkout
        // ==========================
        public IActionResult Checkout()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = HttpContext.Session.GetString("UserEmail")!;

            var cartItems = _mongoDbService.Carts
                .Find(x => x.UserEmail == email)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            return View(cartItems);
        }

        // ==========================
        // Place Order
        // ==========================
        [HttpPost]
        public IActionResult PlaceOrder(string paymentMethod)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = HttpContext.Session.GetString("UserEmail")!;

            var cartItems = _mongoDbService.Carts
                .Find(x => x.UserEmail == email)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            // --------------------------
            // Cash on Delivery
            // --------------------------
            if (paymentMethod == "Cash on Delivery")
            {
                Order order = new Order
                {
                    UserEmail = email,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    PaymentMethod = paymentMethod,
                    PaymentStatus = "Pending",
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

                TempData["Success"] = "Order placed successfully.";

                return RedirectToAction("MyOrders");
            }

            // --------------------------
            // Online Payment
            // --------------------------
            HttpContext.Session.SetString("PaymentMethod", paymentMethod);

            if (paymentMethod == "bKash")
            {
                return RedirectToAction("BKash", "Payment");
            }

            if (paymentMethod == "Nagad")
            {
                return RedirectToAction("Nagad", "Payment");
            }

            if (paymentMethod == "Credit/Debit Card")
            {
                return RedirectToAction("Card", "Payment");
            }

            return RedirectToAction("Checkout");
        }

        // ==========================
        // My Orders
        // ==========================
        public IActionResult MyOrders()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = HttpContext.Session.GetString("UserEmail")!;

            var orders = _mongoDbService.Orders
                .Find(x => x.UserEmail == email)
                .SortByDescending(x => x.OrderDate)
                .ToList();

            return View(orders);
        }

        // ==========================
        // Order Details
        // ==========================
        public IActionResult Details(string id)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _mongoDbService.Orders
                .Find(x => x.Id == id)
                .FirstOrDefault();

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // ==========================
        // Admin - All Orders
        // ==========================
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = _mongoDbService.Orders
                .Find(_ => true)
                .SortByDescending(x => x.OrderDate)
                .ToList();

            return View(orders);
        }

        // ==========================
        // Admin Update Status
        // ==========================
        [HttpPost]
        public IActionResult UpdateStatus(string id, string status)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            var update = Builders<Order>.Update
                .Set(x => x.Status, status);

            _mongoDbService.Orders.UpdateOne(
                x => x.Id == id,
                update);

            TempData["Success"] = "Order status updated successfully.";

            return RedirectToAction("Index");
        }
    }
}