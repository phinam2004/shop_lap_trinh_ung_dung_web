using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.DataLayers.SQLServer;
using SV22T1020673.Models.Common;

namespace SV22T1020673.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng thống kê cho Dashboard
    /// </summary>
    public static class DashboardDataService
    {
        private static readonly IDashboardRepository dashboardDB;

        static DashboardDataService()
        {
            dashboardDB = new SqlDashboardRepository(Configuration.ConnectionString);
        }

        /// <summary>
        /// Lấy toàn bộ dữ liệu thống kê cho Dashboard
        /// </summary>
        /// <returns></returns>
        public static async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            return await dashboardDB.GetDashboardDataAsync();
        }
    }
}
