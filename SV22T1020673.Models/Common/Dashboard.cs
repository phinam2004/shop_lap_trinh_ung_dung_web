using SV22T1020673.Models.Sales;

namespace SV22T1020673.Models.Common
{
    /// <summary>
    /// Chứa các thông tin thống kê hiển thị trên Dashboard
    /// </summary>
    public class DashboardViewModel
    {
        public decimal TotalRevenueToday { get; set; }
        public int NewOrderCount { get; set; }
        public int TotalCustomerCount { get; set; }
        public int TotalProductCount { get; set; }
        public List<MonthlyRevenue> MonthlyRevenues { get; set; } = new();
        public List<TopProduct> TopProducts { get; set; } = new();
        public List<OrderSearchInfo> PendingOrders { get; set; } = new();
    }

    public class MonthlyRevenue { 
        public int Month { get; set; } 
        public int Year { get; set; }
        public decimal Total { get; set; } 
    }

    public class TopProduct { 
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; } 
    }
}
