using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Sales;
using SV22T1020673.Models.DataDictionary;
using SV22T1020673.Shop.Controllers;
using System.Text.Json;

namespace SV22T1020673.Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IDataDictionaryRepository<Province> _provinceRepository;
        private const string CART_SESSION_KEY = "ShoppingCart";

        public OrderController(IOrderRepository orderRepository, 
                               ICustomerRepository customerRepository,
                               IDataDictionaryRepository<Province> provinceRepository)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _provinceRepository = provinceRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index", "Cart");

            int? customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null) return RedirectToAction("Login", "Account");

            var customer = await _customerRepository.GetAsync(customerId.Value);
            ViewBag.Cart = cart;
            ViewBag.Provinces = await _provinceRepository.ListAsync();
            ViewBag.CustomerPhone = customer?.Phone;
            
            var order = new Order
            {
                CustomerID = customerId,
                DeliveryProvince = customer?.Province,
                DeliveryAddress = customer?.Address
            };

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order data, string deliveryPhone)
        {
            var cart = GetCart();
            if (cart.Count == 0) return RedirectToAction("Index", "Cart");

            int? customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null) return RedirectToAction("Login", "Account");

            data.CustomerID = customerId;
            data.OrderTime = DateTime.Now;
            data.Status = OrderStatusEnum.New;

            // Cập nhật số điện thoại khách hàng nếu có thay đổi
            var customer = await _customerRepository.GetAsync(customerId.Value);
            if (customer != null && !string.IsNullOrEmpty(deliveryPhone) && customer.Phone != deliveryPhone)
            {
                customer.Phone = deliveryPhone;
                await _customerRepository.UpdateAsync(customer);
            }

            // 1. Tạo đơn hàng
            int orderId = await _orderRepository.AddAsync(data);

            // 2. Tạo chi tiết đơn hàng
            foreach (var item in cart)
            {
                await _orderRepository.AddDetailAsync(new OrderDetail
                {
                    OrderID = orderId,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.Price
                });
            }

            // 3. Xóa giỏ hàng
            HttpContext.Session.Remove(CART_SESSION_KEY);

            return RedirectToAction("Details", new { id = orderId });
        }

        public async Task<IActionResult> History(OrderSearchInput input)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null) return RedirectToAction("Login", "Account");

            input.CustomerID = customerId.Value;
            input.PageSize = 10;
            if (input.Page <= 0) input.Page = 1;

            var result = await _orderRepository.ListAsync(input);
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetAsync(id);
            if (order == null) return NotFound();

            // Đảm bảo khách hàng chỉ xem được đơn hàng của chính mình
            int? customerId = HttpContext.Session.GetInt32("CustomerID");
            if (order.CustomerID != customerId) return Forbid();

            var details = await _orderRepository.ListDetailsAsync(id);
            ViewBag.Details = details;
            ViewBag.Provinces = await _provinceRepository.ListAsync();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDelivery(int id, string deliveryAddress, string deliveryProvince, string deliveryPhone)
        {
            var order = await _orderRepository.GetAsync(id);
            if (order == null) return Json(new { success = false, message = "Đơn hàng không tồn tại" });

            // Kiểm tra quyền sở hữu
            int? customerId = HttpContext.Session.GetInt32("CustomerID");
            if (order.CustomerID != customerId) return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này" });

            // Kiểm tra trạng thái: Chỉ được sửa khi là New (1) hoặc Accepted (2)
            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
            {
                return Json(new { success = false, message = "Đơn hàng đang giao hoặc đã hoàn tất, không thể chỉnh sửa thông tin" });
            }

            // Cập nhật thông tin đơn hàng
            order.DeliveryAddress = deliveryAddress;
            order.DeliveryProvince = deliveryProvince;
            await _orderRepository.UpdateAsync(order);

            // Cập nhật số điện thoại khách hàng
            if (customerId != null && !string.IsNullOrEmpty(deliveryPhone))
            {
                var customer = await _customerRepository.GetAsync(customerId.Value);
                if (customer != null && customer.Phone != deliveryPhone)
                {
                    customer.Phone = deliveryPhone;
                    await _customerRepository.UpdateAsync(customer);
                }
            }

            return Json(new { success = true });
        }

        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CART_SESSION_KEY);
            if (string.IsNullOrEmpty(cartJson)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }
    }
}
