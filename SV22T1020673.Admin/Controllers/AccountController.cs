using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020673.BusinessLayers;
using SV22T1020673.Models.Security;

namespace SV22T1020673.Admin.Controllers
{
    // Các chức năng liên quan đến tài khoản
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// Giao diện đăng nhập
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>

        [HttpPost]
        [AllowAnonymous]

        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.Username = username;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập đủ tên và mật khẩu");
                return View();
            }

            // Thực hiện kiểm tra thông tin đăng nhập từ database
            var userAccount = await UserAccountService.AuthorizeAsync(username, password);

            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại");
                return View();
            }

            //Dữ liệu sẽ dùng để "ghi" vào giấy chứng nhận (principal)
            var userData = new WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName= userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = userAccount.RoleNames.Split(',').ToList(),
                
            };

            //Thiết lập phiên đăng nhập (cấp giấy chứng nhận)
            await HttpContext.SignInAsync
                (
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    userData.CreatePrincipal()
                );

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Đăng Xuẩt
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ thông tin");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("Error", "Xác nhận mật khẩu mới không khớp");
                return View();
            }

            // Lấy username từ Identity
            var userData = User.GetUserData();
            string userName = userData?.UserName ?? "";

            // Kiểm tra mật khẩu cũ
            var userAccount = await UserAccountService.AuthorizeAsync(userName, oldPassword);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ không chính xác");
                return View();
            }

            // Cập nhật mật khẩu mới
            bool success = await UserAccountService.ChangePasswordAsync(userName, newPassword);
            if (success)
            {
                ViewBag.Message = "Đổi mật khẩu thành công";
                return View();
            }
            else
            {
                ModelState.AddModelError("Error", "Lỗi khi cập nhật mật khẩu");
                return View();
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
