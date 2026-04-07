using SV22T1020673.BusinessLayers;
using SV22T1020673.Models.Common;
using SV22T1020673.Models.Sales;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SV22T1020673.Admin
{
    /// <summary>
    /// Lớp cung cấp các hàm tiện ích dùng cho SelectList (DropDownList)
    /// </summary>
    public static class SelectListHelper
    {
        /// <summary>
        /// Tỉnh thành
        /// </summary>
        /// <returns></returns>
        public static async Task<List<SelectListItem>> Provinces()
        {
            var list = new List<SelectListItem>();
            var result = await DictionaryDataService.ListProvincesAsync();
            foreach (var item in result)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.ProvinceName,
                    Text = item.ProvinceName
                });
            }
            return list;
        }

        /// <summary>
        /// Loại hàng
        /// </summary>
        /// <returns></returns>
        public static async Task<List<SelectListItem>> Categories()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value = "0", Text = "-- Chọn tất cả --" });
            // PageSize phải > 0 (SQL FETCH NEXT không chấp nhận 0)
            var input = new PaginationSearchInput() { Page = 1, PageSize = 10_000, SearchValue = "" };
            var result = await CatalogDataService.ListCategoriesAsync(input);
            foreach (var item in result.DataItems)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.CategoryID.ToString(),
                    Text = item.CategoryName
                });
            }    
            return list;
        }

        /// <summary>
        /// Nhà cung cấp
        /// </summary>
        /// <returns></returns>
        public static async Task<List<SelectListItem>> Suppliers()
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value = "0", Text = "-- Chọn tất cả --" });
            var input = new PaginationSearchInput() { Page = 1, PageSize = 10_000, SearchValue = "" };
            var result = await PartnerDataService.ListSuppliersAsync(input);
            foreach (var item in result.DataItems)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.SupplierID.ToString(),
                    Text = item.SupplierName
                });
            }
            return list;
        }

        /// <summary>
        /// Khách hàng
        /// </summary>
        /// <returns></returns>
        public static async Task<List<SelectListItem>> Customers()
        {
            var list = new List<SelectListItem>();
            var input = new PaginationSearchInput() { Page = 1, PageSize = 10_000, SearchValue = "" };
            var result = await PartnerDataService.ListCustomersAsync(input);
            foreach (var item in result.DataItems)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.CustomerID.ToString(),
                    Text = item.CustomerName
                });
            }
            return list;
        }

        public static async Task<List<SelectListItem>> Shippers()
        {
            var list = new List<SelectListItem>();
            var input = new PaginationSearchInput() { Page = 1, PageSize = 10_000, SearchValue = "" };
            var result = await PartnerDataService.ListShippersAsync(input);
            foreach (var item in result.DataItems)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.ShipperID.ToString(),
                    Text = item.ShipperName
                });
            }
            return list;
        }
    }
}
