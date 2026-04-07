using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.DataLayers.SQLServer;
using SV22T1020673.Models.Catalog;
using SV22T1020673.Models.DataDictionary;

namespace SV22T1020673.Shop
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddShopRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("LiteCommerceDB")!;
            services.AddScoped<ICustomerRepository>(sp => new CustomerRepository(connectionString));
            services.AddScoped<IProductRepository>(sp => new ProductRepository(connectionString));
            services.AddScoped<IGenericRepository<Category>>(sp => new CategoryRepository(connectionString));
            services.AddScoped<IOrderRepository>(sp => new OrderRepository(connectionString));
            services.AddScoped<IUserAccountRepository>(sp => new UserAccountRepository(connectionString));
            services.AddScoped<IDataDictionaryRepository<Province>>(sp => new ProvinceRepository(connectionString));
            return services;
        }
    }
}
