using Dapper;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Common;
using SV22T1020673.Models.Sales;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SV22T1020673.DataLayers.SQLServer
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string connectionString;

        public OrderRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private IDbConnection OpenConnection()
        {
            IDbConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public async Task<PagedResult<OrderSearchInfo>> ListAsync(OrderSearchInput input)
        {
            using (var connection = OpenConnection())
            {
                int rowCount;
                List<OrderSearchInfo> data;

                var sql = @"SELECT COUNT(*)
                            FROM Orders o
                            LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                            WHERE (c.CustomerName LIKE @searchValue OR @searchValue = '')
                              AND (o.Status = @status OR @status = 0)
                              AND (o.OrderTime >= @dateFrom OR @dateFrom IS NULL)
                              AND (o.OrderTime <= @dateTo OR @dateTo IS NULL)
                              AND (o.CustomerID = @customerID OR @customerID = 0);

                            SELECT o.OrderID, o.OrderTime, o.AcceptTime, o.Status,
                                   c.CustomerName, c.Phone AS CustomerPhone,
                                   e.FullName AS EmployeeName,
                                   (SELECT SUM(Quantity * SalePrice) 
                                    FROM OrderDetails 
                                    WHERE OrderID = o.OrderID) AS SumOfPrice
                            FROM Orders o
                            LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                            LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                            WHERE (c.CustomerName LIKE @searchValue OR @searchValue = '')
                              AND (o.Status = @status OR @status = 0)
                              AND (o.OrderTime >= @dateFrom OR @dateFrom IS NULL)
                              AND (o.OrderTime <= @dateTo OR @dateTo IS NULL)
                              AND (o.CustomerID = @customerID OR @customerID = 0)
                            ORDER BY o.OrderTime DESC
                            OFFSET (@page - 1) * @pageSize ROWS
                            FETCH NEXT @pageSize ROWS ONLY";

                using (var multi = await connection.QueryMultipleAsync(sql, new
                {
                    page = input.Page,
                    pageSize = input.PageSize,
                    searchValue = "%" + input.SearchValue + "%",
                    customerID = input.CustomerID,
                    status = (int)input.Status,
                    dateFrom = input.DateFrom,
                    dateTo = input.DateTo
                }))
                {
                    rowCount = multi.Read<int>().Single();
                    data = multi.Read<OrderSearchInfo>().ToList();
                }

                return new PagedResult<OrderSearchInfo>()
                {
                    Page = input.Page,
                    PageSize = input.PageSize,
                    RowCount = rowCount,
                    DataItems = data
                };
            }
        }

        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT o.*, 
                                   c.CustomerName, c.ContactName AS CustomerContactName, c.Email AS CustomerEmail, c.Address AS CustomerAddress, c.Phone AS CustomerPhone,
                                   e.FullName AS EmployeeName,
                                   s.ShipperName, s.Phone AS ShipperPhone
                            FROM Orders o
                            LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                            LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                            LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                            WHERE o.OrderID = @orderID";
                return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { orderID });
            }
        }

        public async Task<int> AddAsync(Order data)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO Orders
                            (CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, Status)
                            VALUES
                            (@CustomerID, @OrderTime, @DeliveryProvince, @DeliveryAddress, @Status);
                            SELECT CAST(SCOPE_IDENTITY() as int);";

                return await connection.ExecuteScalarAsync<int>(sql, data);
            }
        }

        public async Task<bool> UpdateAsync(Order data)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"UPDATE Orders
                            SET CustomerID = @CustomerID,
                                DeliveryProvince = @DeliveryProvince,
                                DeliveryAddress = @DeliveryAddress,
                                EmployeeID = @EmployeeID,
                                AcceptTime = @AcceptTime,
                                ShipperID = @ShipperID,
                                ShippedTime = @ShippedTime,
                                FinishedTime = @FinishedTime,
                                Status = @Status
                            WHERE OrderID = @OrderID";

                int result = await connection.ExecuteAsync(sql, data);
                return result > 0;
            }
        }

        public async Task<bool> DeleteAsync(int orderID)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM Orders WHERE OrderID = @orderID";
                int result = await connection.ExecuteAsync(sql, new { orderID });
                return result > 0;
            }
        }

        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT d.OrderID, d.ProductID, p.ProductName,
                                   d.Quantity, d.SalePrice
                            FROM OrderDetails d
                            JOIN Products p ON d.ProductID = p.ProductID
                            WHERE d.OrderID = @orderID";

                return (await connection.QueryAsync<OrderDetailViewInfo>(sql, new { orderID })).ToList();
            }
        }

        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT d.OrderID, d.ProductID, p.ProductName,
                                   d.Quantity, d.SalePrice
                            FROM OrderDetails d
                            JOIN Products p ON d.ProductID = p.ProductID
                            WHERE d.OrderID = @orderID AND d.ProductID = @productID";

                return await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(sql,
                    new { orderID, productID });
            }
        }

        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO OrderDetails
                            (OrderID, ProductID, Quantity, SalePrice)
                            VALUES
                            (@OrderID, @ProductID, @Quantity, @SalePrice)";

                int result = await connection.ExecuteAsync(sql, data);
                return result > 0;
            }
        }

        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"UPDATE OrderDetails
                            SET Quantity = @Quantity,
                                SalePrice = @SalePrice
                            WHERE OrderID = @OrderID AND ProductID = @ProductID";

                int result = await connection.ExecuteAsync(sql, data);
                return result > 0;
            }
        }

        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM OrderDetails
                            WHERE OrderID = @orderID AND ProductID = @productID";

                int result = await connection.ExecuteAsync(sql, new { orderID, productID });
                return result > 0;
            }
        }
    }
}