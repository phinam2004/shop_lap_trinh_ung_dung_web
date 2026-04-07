using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020673.BusinessLayers;
using SV22T1020673.Models.Sales;
using SV22T1020673.Admin;
using SV22T1020673.Models.Catalog;

namespace SV22T1020673.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến nghiệp vụ bán hàng
    /// </summary>
    [Authorize(Roles = $"{WebUserRoles.Administrator},{WebUserRoles.Sales}")]

    public class OrderController : Controller
    {
        private const int PAGESIZE = 10;
        private const string ORDER_SEARCH_INPUT = "OrderSearchInput";

        /// <summary>
        /// Nhập đầu vào tìm kiếm đơn hàng và hiển thị kết quả tìm kiếm
        /// </summary>
        /// <returns></returns>

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH_INPUT);
            if (input == null)
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = "",
                    Status = (OrderStatusEnum)0,
                    DateFrom = null,
                    DateTo = null
                };

            return View(input);
        }

        /// <summary>
        /// Tìm kiếm và hiển thị danh sách đơn hàng
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            var result = await SalesDataService.ListOrdersAsync(input);
            ApplicationContext.SetSessionData(ORDER_SEARCH_INPUT, input);
            return View(result);
        }

        private const string SEARCH_PRODUCT = "SearchProductToSale";
        /// <summary>
        /// Giao diện thực hiện các chức năng để lập đơn hàng mới
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(SEARCH_PRODUCT);
            if(input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = 3,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0,
                };
            }
            return View(input);
        }

        public async Task<IActionResult> SearchProduct(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(SEARCH_PRODUCT, input);
            return View(result);
        }

        public IActionResult ShowCart(int id)
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            return View(cart);
        }

        /// <summary>
        /// HIển thị thông tin của một đơn hàng và diều hướng đến các chức năng 
        /// xử lý đơn hàng
        /// </summary>
        /// <param name="id">Mã của đơn hàng</param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(int id = 0)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return RedirectToAction("Index");

            var details = await SalesDataService.ListDetailsAsync(id);
            ViewBag.OrderDetails = details;

            return View(order);
        }

        /// <summary>
        /// Giao diện để chỉnh sửa thông tin đơn hàng (địa chỉ giao hàng, tỉnh thành)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditDetail(int id = 0)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return Json(new ApiResult(0, "Đơn hàng không tồn tại"));

            if (order.Status != OrderStatusEnum.New)
                return Json(new ApiResult(0, "Chỉ được sửa đơn hàng khi ở trạng thái 'Vừa khởi tạo'"));

            return PartialView(order);
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng (địa chỉ giao hàng, tỉnh thành)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deliveryProvince"></param>
        /// <param name="deliveryAddress"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateDetail(int id, string deliveryProvince, string deliveryAddress)
        {
            if (string.IsNullOrWhiteSpace(deliveryProvince))
                return Json(new ApiResult(0, "Vui lòng chọn tỉnh thành"));
            if (string.IsNullOrWhiteSpace(deliveryAddress))
                return Json(new ApiResult(0, "Vui lòng nhập địa chỉ"));

            try
            {
                var data = await SalesDataService.GetOrderAsync(id);
                if (data == null)
                    return Json(new ApiResult(0, "Đơn hàng không tồn tại"));

                data.DeliveryProvince = deliveryProvince;
                data.DeliveryAddress = deliveryAddress;

                bool result = await SalesDataService.UpdateOrderAsync(data);
                if (result)
                    return Json(new ApiResult(1, "Cập nhật thông tin đơn hàng thành công"));
                
                return Json(new ApiResult(0, "Cập nhật thông tin đơn hàng thất bại (có thể trạng thái đơn hàng không cho phép)"));
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(0, "Đã xảy ra lỗi: " + ex.Message));
            }
        }


        /// <summary>
        /// Thêm mặt hàng vào giỏ
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddCartItem(int id = 0, int productId = 0, int quantity = 0, decimal price = 0)
        {
            if (quantity <= 0)
                return Json(new ApiResult(0, "Số lượng không hợp lệ"));
            if (price < 0)
                return Json(new ApiResult(0, "Giá bán không hợp lệ"));

            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null) {
                return Json(new ApiResult (0, "Mặt hàng không tồn tại" ));
            }
            if(!product.IsSelling)
            {
                return Json(new ApiResult(0, "Mặt hàng đã ngưng bán" ));
            }
            //Thêm hàng vào giỏ
            var item = new OrderDetailViewInfo()
            {
                ProductID = productId,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Photo = product.Photo ?? "nophoto.png",
                Quantity = quantity,
                SalePrice = price
            };
            ShoppingCartHelper.AddToCart(item);
            return Json(new ApiResult(1, "Thêm mặt hàng vào giỏ thành công"));
        }
        /// <summary>
        /// Cập nhật thông tin của một mặt hàng trong giỏ hàng hoặc trong đơn hàng đã tồn tại
        /// </summary>
        /// <param name="id">0: Xử lý giỏ hàng, khác 0: xử lý cho đơn hàng</param>
        /// <param name="productId">Mã mặt hàng cần xử lý</param>
        /// <returns></returns>
        public IActionResult EditCartItem(int id = 0, int productId = 0)
        {
            var item = ShoppingCartHelper.GetCartItem(productId);
            if (item == null)
                return Json(new ApiResult(0, "Mặt hàng không tồn tại trong giỏ hàng"));

            var model = new CartItem()
            {
                ProductID = item.ProductID,
                ProductName = item.ProductName,
                Unit = item.Unit,
                Photo = item.Photo,
                Quantity = item.Quantity,
                SalePrice = item.SalePrice
            };
            return PartialView(model);
        }

        [HttpPost]
        public IActionResult UpdateCartItem(int productId, int quantity, string salePrice)
        {
            if (quantity <= 0)
                return Json(new ApiResult(0, "Số lượng không hợp lệ"));

            // Chuyển đổi giá bán từ chuỗi (có thể chứa dấu phân cách nghìn) sang decimal
            decimal price = 0;
            try
            {
                price = decimal.Parse(salePrice, System.Globalization.NumberStyles.Any);
            }
            catch
            {
                return Json(new ApiResult(0, "Giá bán không hợp lệ"));
            }

            if (price < 0)
                return Json(new ApiResult(0, "Giá bán không hợp lệ"));

            ShoppingCartHelper.UpdateCartItem(productId, quantity, price);
            return Json(new ApiResult(1, "Cập nhật giỏ hàng thành công"));
        }
        /// <summary>
        /// Xóa mặt hàng ra khỏi giỏ hàng hoặc ra khỏi đơn hàng
        /// </summary>
        /// <param name="id">0: Xử lý giỏ hàng, khác 0: xử lý cho đơn hàng</param>
        /// <param name="productId">Mã mặt hàng cần xử lý</param>
        /// <returns></returns>        
        public IActionResult DeleteCartItem(int id = 0, int productId = 0)
        {
            //POST: Xóa mặt hàng khỏi giỏ hàng
            if (Request.Method == "POST")
            {
                ShoppingCartHelper.RemoveItemFromCart(productId);
                return Json(new ApiResult(1, "Xóa mặt hàng khỏi giỏ hàng thành công"));
            }
            //GET: Hiển thị hộp thoại xác nhận xóa
            var item = ShoppingCartHelper.GetCartItem(productId);
            if (item == null)
                return Json(new ApiResult(0, "Mặt hàng không tồn tại trong giỏ hàng"));

            var model = new CartItem()
            {
                ProductID = item.ProductID,
                ProductName = item.ProductName,
                Unit = item.Unit,
                Photo = item.Photo,
                Quantity = item.Quantity,
                SalePrice = item.SalePrice
            };
            ViewBag.ProductID = productId;
            return PartialView(model);
        }
        /// <summary>
        /// Xóa giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult ClearCart()
        {
            //POST: Xóa giỏ hàng
            if (Request.Method == "POST")
            {
                ShoppingCartHelper.ClearCart();
                return Json(new ApiResult(1, "Xóa giỏ hàng thành công"));
            }
            //GET: Hiển thị giao diện để xác nhận
            return PartialView();
        }
        #region Xem và xử lý đơn hàng
        /// <summary>
        /// Duyệt chấp nhận đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần duyệt</param>
        /// <returns></returns>        
        public IActionResult Accept(int id)
        {
            ViewBag.OrderID = id;
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> Accept(int id, string _ = "")
        {
            int employeeID = int.Parse(User.GetUserData()?.UserId ?? "0");
            bool result = await SalesDataService.AcceptOrderAsync(id, employeeID);
            if (result)
                return Json(new ApiResult(1, "Duyệt đơn hàng thành công"));
            return Json(new ApiResult(0, "Không thể duyệt đơn hàng này"));
        }
        /// <summary>
        /// Chuyển hàng cho người giao hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển </param>
        /// <returns></returns>
        public IActionResult Shipping(int id)
        {
            ViewBag.OrderID = id;
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> Shipping(int id, int shipperID = 0)
        {
            if (shipperID <= 0)
                return Json(new ApiResult(0, "Vui lòng chọn người giao hàng"));

            bool result = await SalesDataService.ShipOrderAsync(id, shipperID);
            if (result)
                return Json(new ApiResult(1, "Chuyển đơn hàng sang trạng thái đang giao thành công"));
            return Json(new ApiResult(0, "Không thể chuyển trạng thái đơn hàng này"));
        }
        /// <summary>
        /// Kết thúc đơn hàng 
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Finish(int id)
        {
            ViewBag.OrderID = id;
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> Finish(int id, string _ = "")
        {
            bool result = await SalesDataService.CompleteOrderAsync(id);
            if (result)
                return Json(new ApiResult(1, "Xác nhận hoàn tất đơn hàng thành công"));
            return Json(new ApiResult(0, "Không thể hoàn tất đơn hàng này"));
        }
        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Reject(int id)
        {
            ViewBag.OrderID = id;
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> Reject(int id, string _ = "")
        {
            int employeeID = int.Parse(User.GetUserData()?.UserId ?? "0");
            bool result = await SalesDataService.RejectOrderAsync(id, employeeID);
            if (result)
                return Json(new ApiResult(1, "Từ chối đơn hàng thành công"));
            return Json(new ApiResult(0, "Không thể từ chối đơn hàng này"));
        }
        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        /// <param name="id">Mã đươn hàng</param>
        /// <returns></returns>
        public IActionResult Cancel(int id)
        {
            ViewBag.OrderID = id;
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string _ = "")
        {
            bool result = await SalesDataService.CancelOrderAsync(id);
            if (result)
                return Json(new ApiResult(1, "Hủy đơn hàng thành công"));
            return Json(new ApiResult(0, "Không thể hủy đơn hàng này"));
        }
        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            ViewBag.OrderID = id;
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string _ = "")
        {
            bool result = await SalesDataService.DeleteOrderAsync(id);
            if (result)
                return Json(new ApiResult(1, "Xóa đơn hàng thành công"));
            return Json(new ApiResult(0, "Không thể xóa đơn hàng này (đơn hàng có thể đã được xử lý)"));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(int customerID = 0, string province = "", string address= "")
        {
            if (customerID <= 0)
                return Json(new ApiResult(0, "Vui lòng chọn khách hàng"));
            if (string.IsNullOrWhiteSpace(province))
                return Json(new ApiResult(0, "Vui lòng chọn tỉnh/thành giao hàng"));
            if (string.IsNullOrWhiteSpace(address))
                return Json(new ApiResult(0, "Vui lòng nhập địa chỉ giao hàng"));

            var cart = ShoppingCartHelper.GetShoppingCart();
            if(cart.Count == 0)
                return Json(new ApiResult(0, "Giỏ hàng không có mặt hàng nào"));

            //Lập đơn hàng và ghi chi tiết của đơn hàng
            int orderID = await SalesDataService.AddOrderAsync(customerID, province, address, cart.Select(item => new OrderDetail()
            {
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                SalePrice = item.SalePrice
            }));
            ShoppingCartHelper.ClearCart();
            return Json(new ApiResult(orderID, "Lập đơn hàng thành công"));
        }
        #endregion
    }
}