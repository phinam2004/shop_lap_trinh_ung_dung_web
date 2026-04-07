using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Catalog;
using SV22T1020673.Models.Common;

namespace SV22T1020673.DataLayers.SQLServer
{
    public class ProductRepository : IProductRepository
    {
        private readonly string connectionString;

        public ProductRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        #region PRODUCT

        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            using var connection = GetConnection();

            string sql = @"
                        SELECT COUNT(*)
                        FROM Products
                        WHERE (@search = '%%' OR ProductName LIKE @search)
                          AND (@categoryID = 0 OR CategoryID = @categoryID)
                          AND (@supplierID = 0 OR SupplierID = @supplierID)
                          AND (@minPrice = 0 OR Price >= @minPrice)
                          AND (@maxPrice = 0 OR Price <= @maxPrice);

                        SELECT *
                        FROM Products
                        WHERE (@search = '%%' OR ProductName LIKE @search)
                          AND (@categoryID = 0 OR CategoryID = @categoryID)
                          AND (@supplierID = 0 OR SupplierID = @supplierID)
                          AND (@minPrice = 0 OR Price >= @minPrice)
                          AND (@maxPrice = 0 OR Price <= @maxPrice)
                        ORDER BY ProductName
                        OFFSET @offset ROWS FETCH NEXT @pagesize ROWS ONLY";

            var param = new
            {
                search = $"%{input.SearchValue}%",
                categoryID = input.CategoryID,
                supplierID = input.SupplierID,
                minPrice = input.MinPrice,
                maxPrice = input.MaxPrice,
                offset = (input.Page - 1) * input.PageSize,
                pagesize = input.PageSize
            };

            using var multi = await connection.QueryMultipleAsync(sql, param);

            int count = multi.Read<int>().Single();
            var data = multi.Read<Product>().ToList();

            return new PagedResult<Product>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = count,
                DataItems = data
            };
        }

        public async Task<Product?> GetAsync(int productID)
        {
            using var connection = GetConnection();

            string sql = "SELECT * FROM Products WHERE ProductID=@productID";

            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { productID });
        }

        public async Task<int> AddAsync(Product data)
        {
            using var connection = GetConnection();

            string sql = @"
                        INSERT INTO Products(ProductName,ProductDescription,SupplierID,CategoryID,Unit,Price,Photo,IsSelling)
                        VALUES(@ProductName,@ProductDescription,@SupplierID,@CategoryID,@Unit,@Price,@Photo,@IsSelling);
                        SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Product data)
        {
            using var connection = GetConnection();

            string sql = @"
                        UPDATE Products
                        SET ProductName=@ProductName,
                            ProductDescription=@ProductDescription,
                            SupplierID=@SupplierID,
                            CategoryID=@CategoryID,
                            Unit=@Unit,
                            Price=@Price,
                            Photo=@Photo,
                            IsSelling=@IsSelling
                        WHERE ProductID=@ProductID";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> DeleteAsync(int productID)
        {
            using var connection = GetConnection();

            string sql = "DELETE FROM Products WHERE ProductID=@productID";

            return await connection.ExecuteAsync(sql, new { productID }) > 0;
        }

        public async Task<bool> IsUsedAsync(int productID)
        {
            using var connection = GetConnection();

            string sql = "SELECT COUNT(*) FROM OrderDetails WHERE ProductID=@productID";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { productID });

            return count > 0;
        }

        #endregion


        #region ATTRIBUTE

        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            using var connection = GetConnection();

            string sql = "SELECT * FROM ProductAttributes WHERE ProductID=@productID";

            var data = await connection.QueryAsync<ProductAttribute>(sql, new { productID });

            return data.ToList();
        }

        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            using var connection = GetConnection();

            string sql = "SELECT * FROM ProductAttributes WHERE AttributeID=@attributeID";

            return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { attributeID });
        }

        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using var connection = GetConnection();

            string sql = @"
                        INSERT INTO ProductAttributes(ProductID,AttributeName,AttributeValue,DisplayOrder)
                        VALUES(@ProductID,@AttributeName,@AttributeValue,@DisplayOrder);
                        SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using var connection = GetConnection();

            string sql = @"
                        UPDATE ProductAttributes
                        SET AttributeName=@AttributeName,
                            AttributeValue=@AttributeValue,
                            DisplayOrder=@DisplayOrder
                        WHERE AttributeID=@AttributeID";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using var connection = GetConnection();

            string sql = "DELETE FROM ProductAttributes WHERE AttributeID=@attributeID";

            return await connection.ExecuteAsync(sql, new { attributeID }) > 0;
        }

        #endregion


        #region PHOTO

        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            using var connection = GetConnection();

            string sql = "SELECT * FROM ProductPhotos WHERE ProductID=@productID";

            var data = await connection.QueryAsync<ProductPhoto>(sql, new { productID });

            return data.ToList();
        }

        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            using var connection = GetConnection();

            string sql = "SELECT * FROM ProductPhotos WHERE PhotoID=@photoID";

            return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { photoID });
        }

        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            data.Description ??= string.Empty;

            using var connection = GetConnection();

            string sql = @"
                        INSERT INTO ProductPhotos(ProductID,Photo,Description,DisplayOrder,IsHidden)
                        VALUES(@ProductID,@Photo,@Description,@DisplayOrder,@IsHidden);
                        SELECT SCOPE_IDENTITY();";

            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            data.Description ??= string.Empty;

            using var connection = GetConnection();

            string sql = @"
                        UPDATE ProductPhotos
                        SET Photo=@Photo,
                            Description=@Description,
                            DisplayOrder=@DisplayOrder,
                            IsHidden=@IsHidden
                        WHERE PhotoID=@PhotoID";

            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            using var connection = GetConnection();

            string sql = "DELETE FROM ProductPhotos WHERE PhotoID=@photoID";

            return await connection.ExecuteAsync(sql, new { photoID }) > 0;
        }

        #endregion
    }
}