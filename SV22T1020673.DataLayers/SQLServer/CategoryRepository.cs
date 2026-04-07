using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Common;
using SV22T1020673.Models.Catalog;

namespace SV22T1020673.DataLayers.SQLServer
{
    public class CategoryRepository : IGenericRepository<Category>
    {
        private readonly string _connectionString;

        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<int> AddAsync(Category data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Categories(CategoryName, Description)
                VALUES(@CategoryName, @Description);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Category data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Categories
                SET CategoryName = @CategoryName,
                    Description = @Description
                WHERE CategoryID = @CategoryID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();

            string sql = "DELETE FROM Categories WHERE CategoryID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        public async Task<Category?> GetAsync(int id)
        {
            using var connection = GetConnection();

            string sql = "SELECT * FROM Categories WHERE CategoryID = @id";

            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { id });
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT COUNT(*)
                FROM Products
                WHERE CategoryID = @id
            ";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
            return count > 0;
        }

        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            using var connection = GetConnection();

            var result = new PagedResult<Category>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string countSql = @"
                SELECT COUNT(*)
                FROM Categories
                WHERE CategoryName LIKE @SearchValue
            ";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                SearchValue = $"%{input.SearchValue}%"
            });

            string dataSql = @"
                SELECT *
                FROM Categories
                WHERE CategoryName LIKE @SearchValue
                ORDER BY CategoryName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            ";

            var data = await connection.QueryAsync<Category>(dataSql, new
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