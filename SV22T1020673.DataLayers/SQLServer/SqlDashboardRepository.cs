using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Common;
using SV22T1020673.Models.Sales;
using System.Data;

namespace SV22T1020673.DataLayers.SQLServer
{
    public class SqlDashboardRepository : IDashboardRepository
    {
        private readonly string _connectionString;

        public SqlDashboardRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection GetConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var model = new DashboardViewModel();
            using (var connection = GetConnection())
            {
                var sql = @"
                    -- 1. Doanh thu hôm nay (Chỉ tính các đơn hàng không bị hủy/từ chối)
                    SELECT ISNULL(SUM(od.Quantity * od.SalePrice), 0)
                    FROM Orders o
                    JOIN OrderDetails od ON o.OrderID = od.OrderID
                    WHERE CAST(o.OrderTime AS DATE) = CAST(GETDATE() AS DATE)
                      AND o.Status NOT IN (-1, -2);

                    -- 2. Số lượng đơn hàng mới
                    SELECT COUNT(*) FROM Orders WHERE Status = 1;

                    -- 3. Số lượng khách hàng
                    SELECT COUNT(*) FROM Customers;

                    -- 4. Số lượng mặt hàng
                    SELECT COUNT(*) FROM Products;

                    -- 5. Doanh thu 6 tháng gần nhất
                    SELECT MONTH(o.OrderTime) as [Month], YEAR(o.OrderTime) as [Year], SUM(od.Quantity * od.SalePrice) as Total
                    FROM Orders o
                    JOIN OrderDetails od ON o.OrderID = od.OrderID
                    WHERE o.OrderTime >= DATEADD(MONTH, -6, GETDATE())
                      AND o.Status NOT IN (-1, -2)
                    GROUP BY YEAR(o.OrderTime), MONTH(o.OrderTime)
                    ORDER BY YEAR(o.OrderTime), MONTH(o.OrderTime);

                    -- 6. Top 5 sản phẩm bán chạy nhất
                    SELECT TOP 5 p.ProductName, SUM(od.Quantity) as Quantity
                    FROM OrderDetails od
                    JOIN Products p ON od.ProductID = p.ProductID
                    JOIN Orders o ON od.OrderID = o.OrderID
                    WHERE o.Status NOT IN (-1, -2)
                    GROUP BY p.ProductName
                    ORDER BY Quantity DESC;

                    -- 7. 5 Đơn hàng mới nhất cần xử lý (Status = 1 hoặc 2)
                    SELECT TOP 5 o.OrderID, o.OrderTime, o.Status,
                                 c.CustomerName,
                                 ISNULL((SELECT SUM(Quantity * SalePrice) FROM OrderDetails WHERE OrderID = o.OrderID), 0) AS SumOfPrice
                    FROM Orders o
                    LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                    WHERE o.Status IN (1, 2)
                    ORDER BY o.OrderTime DESC;
                ";

                using (var multi = await connection.QueryMultipleAsync(sql))
                {
                    model.TotalRevenueToday = await multi.ReadFirstAsync<decimal>();
                    model.NewOrderCount = await multi.ReadFirstAsync<int>();
                    model.TotalCustomerCount = await multi.ReadFirstAsync<int>();
                    model.TotalProductCount = await multi.ReadFirstAsync<int>();
                    
                    // Lấy dữ liệu thô từ DB
                    var rawMonthlyData = (await multi.ReadAsync<MonthlyRevenue>()).ToList();
                    
                    // Đảm bảo luôn lấy đủ 6 tháng gần nhất (bao gồm cả tháng hiện tại)
                    var processedMonthlyData = new List<MonthlyRevenue>();
                    var now = DateTime.Now;
                    for (int i = 5; i >= 0; i--)
                    {
                        var targetDate = now.AddMonths(-i);
                        var existing = rawMonthlyData.FirstOrDefault(r => r.Month == targetDate.Month && r.Year == targetDate.Year);
                        processedMonthlyData.Add(new MonthlyRevenue
                        {
                            Month = targetDate.Month,
                            Year = targetDate.Year,
                            Total = existing?.Total ?? 0
                        });
                    }
                    model.MonthlyRevenues = processedMonthlyData;

                    model.TopProducts = (await multi.ReadAsync<TopProduct>()).ToList();
                    model.PendingOrders = (await multi.ReadAsync<OrderSearchInfo>()).ToList();
                }
            }
            return model;
        }
    }
}
