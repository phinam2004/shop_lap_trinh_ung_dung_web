using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.DataLayers.SQLServer;
using SV22T1020673.Models.Common;
using SV22T1020673.Models.Sales;

namespace SV22T1020673.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến bán hàng
    /// bao gồm: đơn hàng (Order) và chi tiết đơn hàng (OrderDetail).
    /// </summary>
    public static class SalesDataService
    {
        private static readonly IOrderRepository orderDB;

        /// <summary>
        /// Constructor
        /// </summary>
        static SalesDataService()
        {
            orderDB = new OrderRepository(Configuration.ConnectionString);
        }

        #region Order

        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng dưới dạng phân trang
        /// </summary>
        public static async Task<PagedResult<OrderSearchInfo>> ListOrdersAsync(OrderSearchInput input)
        {
            return await orderDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một đơn hàng
        /// </summary>
        public static async Task<OrderViewInfo?> GetOrderAsync(int orderID)
        {
            return await orderDB.GetAsync(orderID);
        }

        /// <summary>
        /// Tạo đơn hàng mới (viết ngu)
        /// </summary>
        public static async Task<int> AddOrderAsync(Order data)
        {
            data.Status = OrderStatusEnum.New;
            data.OrderTime = DateTime.Now;

            return await orderDB.AddAsync(data);
        }

        /// <summary>
        /// Lập đơn hàng (viết đúng)
        /// </summary>
        /// <param name="customerID">Mã khách hàng</param>
        /// <param name="province">Tỉnh/thành giao hàng</param>
        /// <param name="address">Địa chỉ giao hàng</param>
        /// <param name="details">Danh sách chi tiết mặt hàng</param>
        /// <returns>Mã đơn hàng được tạo</returns>
        public static async Task<int> AddOrderAsync(int customerID, string province, string address, IEnumerable<OrderDetail> details)
        {
            var order = new Order()
            {
                CustomerID = customerID == 0 ? null : customerID,
                OrderTime = DateTime.Now,
                DeliveryProvince = province,
                DeliveryAddress = address,
                Status = OrderStatusEnum.New
            };

            int orderID = await orderDB.AddAsync(order);
            foreach (var item in details)
            {
                item.OrderID = orderID;
                await orderDB.AddDetailAsync(item);
            }

            return orderID;
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng
        /// </summary>
        public static async Task<bool> UpdateOrderAsync(Order data)
        {
            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null)
                return false;
            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
                return false;

            return await orderDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        public static async Task<bool> DeleteOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null)
                return false;
            if (order.Status != OrderStatusEnum.New &&
                order.Status != OrderStatusEnum.Rejected &&
                order.Status != OrderStatusEnum.Cancelled)
                return false;

            return await orderDB.DeleteAsync(orderID);
        }

        #endregion

        #region Order Status Processing

        /// <summary>
        /// Duyệt đơn hàng
        /// </summary>
        public static async Task<bool> AcceptOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) 
                return false;

            if (order.Status != OrderStatusEnum.New)
                return false;

            order.EmployeeID = employeeID;
            order.AcceptTime = DateTime.Now;
            order.Status = OrderStatusEnum.Accepted;

            return await orderDB.UpdateAsync(order);
        }

        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        public static async Task<bool> RejectOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) 
                return false;

            if (order.Status != OrderStatusEnum.New)
                return false;

            order.EmployeeID = employeeID;
            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Rejected;
            
            return await orderDB.UpdateAsync(order);
        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        public static async Task<bool> CancelOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) 
                return false;

            if (order.Status != OrderStatusEnum.New &&
                order.Status != OrderStatusEnum.Accepted)
                return false;

            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Cancelled;
            
            return await orderDB.UpdateAsync(order);
        }

        /// <summary>
        /// Giao đơn hàng cho người giao hàng
        /// </summary>
        public static async Task<bool> ShipOrderAsync(int orderID, int shipperID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) 
                return false;

            if (order.Status != OrderStatusEnum.Accepted)
                return false;

            order.ShipperID = shipperID;
            order.ShippedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Shipping;
            
            return await orderDB.UpdateAsync(order);
        }

        /// <summary>
        /// Hoàn tất đơn hàng
        /// </summary>
        public static async Task<bool> CompleteOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) 
                return false;

            if (order.Status != OrderStatusEnum.Shipping)
                return false;

            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Completed;
            
            return await orderDB.UpdateAsync(order);
        }

        #endregion

        #region Order Detail

        /// <summary>
        /// Lấy danh sách mặt hàng của đơn hàng
        /// </summary>
        public static async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            return await orderDB.ListDetailsAsync(orderID);
        }

        /// <summary>
        /// Lấy thông tin một mặt hàng trong đơn hàng
        /// </summary>
        public static async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            return await orderDB.GetDetailAsync(orderID, productID);
        }

        /// <summary>
        /// Thêm mặt hàng vào đơn hàng
        /// </summary>
        public static async Task<bool> AddDetailAsync(OrderDetail data)
        {
            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null)
                return false;
            if (order.Status != OrderStatusEnum.New &&
                order.Status != OrderStatusEnum.Accepted)
                return false;

            return await orderDB.AddDetailAsync(data);
        }

        /// <summary>
        /// Cập nhật mặt hàng trong đơn hàng
        /// </summary>
        public static async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null)
                return false;
            if (order.Status != OrderStatusEnum.New &&
                order.Status != OrderStatusEnum.Accepted)
                return false;

            return await orderDB.UpdateDetailAsync(data);
        }

        /// <summary>
        /// Xóa mặt hàng khỏi đơn hàng
        /// </summary>
        public static async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null)
                return false;
            if (order.Status != OrderStatusEnum.New &&
                order.Status != OrderStatusEnum.Accepted)
                return false;

            return await orderDB.DeleteDetailAsync(orderID, productID);
        }

        #endregion
    }
}