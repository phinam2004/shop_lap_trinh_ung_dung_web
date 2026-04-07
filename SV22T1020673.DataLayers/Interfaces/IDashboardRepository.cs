using SV22T1020673.Models.Common;

namespace SV22T1020673.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu để lấy thông tin cho Dashboard
    /// </summary>
    public interface IDashboardRepository
    {
        /// <summary>
        /// Lấy toàn bộ dữ liệu thống kê cho Dashboard
        /// </summary>
        /// <returns></returns>
        Task<DashboardViewModel> GetDashboardDataAsync();
    }
}
