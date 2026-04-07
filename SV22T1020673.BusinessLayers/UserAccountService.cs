using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.DataLayers.SQLServer;
using SV22T1020673.Models.Security;

namespace SV22T1020673.BusinessLayers
{
    /// <summary>
    /// Các chức năng xử lý liên quan đến tài khoản người dùng
    /// </summary>
    public static class UserAccountService
    {
        private static readonly IUserAccountRepository userAccountDB;

        static UserAccountService()
        {
            userAccountDB = new UserAccountRepository(Configuration.ConnectionString);
        }

        public static async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            // Mã hóa MD5 trước khi kiểm tra trong database
            string passwordHash = HashMD5(password);
            return await userAccountDB.AuthorizeAsync(userName, passwordHash);
        }

        public static async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            // Mã hóa MD5 trước khi cập nhật vào database
            string passwordHash = HashMD5(password);
            return await userAccountDB.ChangePasswordAsync(userName, passwordHash);
        }

        /// <summary>
        /// Mã hóa MD5 chuỗi văn bản
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string HashMD5(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
