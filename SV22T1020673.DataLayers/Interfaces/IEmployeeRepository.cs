using SV22T1020673.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020673.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Employee
    /// </summary>
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        /// <summary>
        /// Kiểm tra xem email của nhân viên có hợp lệ không
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: Kiểm tra email của nhân viên mới
        /// Nếu id <> 0: Kiểm tra email của nhân viên có mã là id
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int id = 0);

        /// <summary>
        /// Cập nhật mật khẩu cho nhân viên
        /// </summary>
        /// <param name="employeeID">Mã nhân viên</param>
        /// <param name="newPassword">Mật khẩu mới (đã mã hóa)</param>
        /// <returns></returns>
        Task<bool> UpdatePasswordAsync(int employeeID, string newPassword);

        /// <summary>
        /// Cập nhật danh sách quyền cho nhân viên
        /// </summary>
        /// <param name="employeeID">Mã nhân viên</param>
        /// <param name="roleNames">Danh sách các quyền (phân tách bằng dấu phẩy)</param>
        /// <returns></returns>
        Task<bool> UpdateRoleNamesAsync(int employeeID, string roleNames);
    }
}
