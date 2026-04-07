using Microsoft.AspNetCore.Mvc;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Catalog;
using System.Text.Json;

namespace SV22T1020673.Shop.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private const string CART_SESSION_KEY = "ShoppingCart";

        public CartController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int id, int quantity = 1)
        {
            var product = await _productRepository.GetAsync(id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductID == id);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Photo = product.Photo,
                    Price = product.Price,
                    Quantity = quantity,
                    Unit = product.Unit
                });
            }

            SaveCart(cart);

            // If it's an AJAX request, return the new total count (unique products)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, count = cart.Count });
            }

            return RedirectToAction("Index");
        }

        public IActionResult GetCount()
        {
            var cart = GetCart();
            return Json(new { count = cart.Count });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductID == id);
            
            if (item != null)
            {
                if (quantity > 0) item.Quantity = quantity;
                else cart.Remove(item);
            }

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductID == id);
            if (item != null) cart.Remove(item);
            
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CART_SESSION_KEY);
            return RedirectToAction("Index");
        }

        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CART_SESSION_KEY);
            if (string.IsNullOrEmpty(cartJson)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CART_SESSION_KEY, JsonSerializer.Serialize(cart));
        }
    }

    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string? Photo { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }
}
