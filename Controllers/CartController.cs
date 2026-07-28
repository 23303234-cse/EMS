using EMS.Models;
using EMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EMS.Controllers
{
    public class CartController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public CartController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // ==========================
        // My Cart
        // ==========================
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = HttpContext.Session.GetString("UserEmail")!;

            var cartItems = _mongoDbService.Carts
                .Find(x => x.UserEmail == email)
                .ToList();

            return View(cartItems);
        }

        // ==========================
        // Add To Cart
        // ==========================
        public IActionResult Add(string id)
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string email = HttpContext.Session.GetString("UserEmail")!;

            var product = _mongoDbService.Products
                .Find(x => x.Id == id)
                .FirstOrDefault();

            if (product == null)
            {
                return RedirectToAction("Index", "Product");
            }

            var existingItem = _mongoDbService.Carts
                .Find(x => x.UserEmail == email && x.ProductId == id)
                .FirstOrDefault();

            if (existingItem != null)
            {
                existingItem.Quantity++;

                _mongoDbService.Carts.ReplaceOne(
                    x => x.Id == existingItem.Id,
                    existingItem);
            }
            else
            {
                Cart cart = new Cart
                {
                    UserEmail = email,
                    ProductId = product.Id!,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Quantity = 1,
                    Image = product.Image
                };

                _mongoDbService.Carts.InsertOne(cart);
            }

            return RedirectToAction("Index");
        }

        // ==========================
        // Increase Quantity
        // ==========================
        public IActionResult Increase(string id)
        {
            var cartItem = _mongoDbService.Carts
                .Find(x => x.Id == id)
                .FirstOrDefault();

            if (cartItem != null)
            {
                cartItem.Quantity++;

                _mongoDbService.Carts.ReplaceOne(
                    x => x.Id == id,
                    cartItem);
            }

            return RedirectToAction("Index");
        }

        // ==========================
        // Decrease Quantity
        // ==========================
        public IActionResult Decrease(string id)
        {
            var cartItem = _mongoDbService.Carts
                .Find(x => x.Id == id)
                .FirstOrDefault();

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;

                    _mongoDbService.Carts.ReplaceOne(
                        x => x.Id == id,
                        cartItem);
                }
                else
                {
                    _mongoDbService.Carts.DeleteOne(x => x.Id == id);
                }
            }

            return RedirectToAction("Index");
        }

        // ==========================
        // Remove From Cart
        // ==========================
        public IActionResult Remove(string id)
        {
            _mongoDbService.Carts.DeleteOne(x => x.Id == id);

            return RedirectToAction("Index");
        }
    }
}