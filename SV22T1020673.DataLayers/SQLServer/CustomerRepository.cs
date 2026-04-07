using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Common;
using SV22T1020673.Models.Partner;

namespace SV22T1020673.DataLayers.SQLServer
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<int> AddAsync(Customer data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Customers
                (CustomerName, ContactName, Province, Address, Phone, Email, IsLocked, [Password])
                VALUES
                (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @IsLocked, @Password);

                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Customer data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Customers
                SET CustomerName = @CustomerName,
                    ContactName = @ContactName,
                    Province = @Province,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email,
                    IsLocked = @IsLocked,
                    [Password] = @Password
                WHERE CustomerID = @CustomerID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();

            string sql = "DELETE FROM Customers WHERE CustomerID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        public async Task<Customer?> GetAsync(int id)
        {
            using var connection = GetConnection();

            string sql = "SELECT * FROM Customers WHERE CustomerID = @id";

            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { id });
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT COUNT(*)
                FROM Orders
                WHERE CustomerID = @id
            ";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
            return count > 0;
        }

        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT COUNT(*)
                FROM Customers
                WHERE Email = @email
                AND CustomerID <> @id
            ";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });

            return count == 0;
        }

        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            using var connection = GetConnection();

            var result = new PagedResult<Customer>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string countSql = @"
                SELECT COUNT(*)
                FROM Customers
                WHERE CustomerName LIKE @SearchValue
                   OR ContactName LIKE @SearchValue
                   OR Email LIKE @SearchValue
            ";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                SearchValue = $"%{input.SearchValue}%"
            });

            string dataSql = @"
                SELECT *
                FROM Customers
                WHERE CustomerName LIKE @SearchValue
                   OR ContactName LIKE @SearchValue
                   OR Email LIKE @SearchValue
                ORDER BY CustomerName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            ";

            var data = await connection.QueryAsync<Customer>(dataSql, new
            {
                SearchValue = $"%{input.SearchValue}%",
                Offset = input.Offset,
                PageSize = input.PageSize
            });

            result.DataItems = data.ToList();

            return result;
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            using var connection = GetConnection();

            string sql = "SELECT * FROM Customers WHERE Email = @email";

            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { email });
        }

        public async Task<bool> UpdatePasswordAsync(int customerID, string newPassword)
        {
            using (var connection = GetConnection())
            {
                var sql = @"UPDATE Customers SET [Password] = @newPassword WHERE CustomerID = @customerID";
                int result = await connection.ExecuteAsync(sql, new { customerID, newPassword });
                return result > 0;
            }
        }
    }
}