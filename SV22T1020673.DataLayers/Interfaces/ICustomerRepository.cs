using SV22T1020673.Models.Partner;

namespace SV22T1020673.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Customer
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Kiểm tra xem một địa chỉ email có hợp lệ hay không?
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: Kiểm tra email của khách hàng mới.
        /// Nếu id <> 0: Kiểm tra email đối với khách hàng đã tồn tại
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int id = 0);

        /// <summary>
        /// Lấy dữ liệu của một khách hàng dựa trên địa chỉ email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<Customer?> GetByEmailAsync(string email);

        /// <summary>
        /// Cập nhật mật khẩu cho khách hàng
        /// </summary>
        /// <param name="customerID">Mã khách hàng</param>
        /// <param name="newPassword">Mật khẩu mới (đã mã hóa)</param>
        /// <returns></returns>
        Task<bool> UpdatePasswordAsync(int customerID, string newPassword);
    }
}
