using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Common;
using SV22T1020673.Models.Partner;

namespace SV22T1020673.DataLayers.SQLServer
{
    public class ShipperRepository : IGenericRepository<Shipper>
    {
        private readonly string _connectionString;

        public ShipperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<int> AddAsync(Shipper data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Shippers(ShipperName, Phone)
                VALUES(@ShipperName, @Phone);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Shipper data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Shippers
                SET ShipperName = @ShipperName,
                    Phone = @Phone
                WHERE ShipperID = @ShipperID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();

            string sql = "DELETE FROM Shippers WHERE ShipperID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        public async Task<Shipper?> GetAsync(int id)
        {
            using var connection = GetConnection();

            string sql = "SELECT * FROM Shippers WHERE ShipperID = @id";

            return await connection.QueryFirstOrDefaultAsync<Shipper>(sql, new { id });
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT COUNT(*)
                FROM Orders
                WHERE ShipperID = @id
            ";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
            return count > 0;
        }

        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            using var connection = GetConnection();

            var result = new PagedResult<Shipper>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string countSql = @"
                SELECT COUNT(*)
                FROM Shippers
                WHERE ShipperName LIKE @SearchValue
            ";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                SearchValue = $"%{input.SearchValue}%"
            });

            string dataSql = @"
                SELECT *
                FROM Shippers
                WHERE ShipperName LIKE @SearchValue
                ORDER BY ShipperName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            ";

            var data = await connection.QueryAsync<Shipper>(dataSql, new
            {
                SearchValue = $"%{input.SearchValue}%",
                Offset = input.Offset,
                PageSize = input.PageSize
            });

            result.DataItems = data.ToList();

            return result;
        }
    }
}