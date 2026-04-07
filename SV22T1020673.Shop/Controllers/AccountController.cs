using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Partner;
using SV22T1020673.Models.DataDictionary;
using SV22T1020673.Models.Security;
using System.Security.Claims;

namespace SV22T1020673.Shop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IDataDictionaryRepository<Province> _provinceRepository;
        
        public AccountController(ICustomerRepository customerRepository, IDataDictionaryRepository<Province> provinceRepository)
        {
            _customerRepository = customerRepository;
            _provinceRepository = provinceRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Home", new { auth = "login" });
        }


        [HttpPost]
        public async Task<IActionResult> AjaxLogin(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ email và mật khẩu" });

            var passwordHash = CryptHelper.HashMD5(password);
            var customer = await _customerRepository.GetByEmailAsync(email);

            if (customer == null || customer.Password != passwordHash)
                return Json(new { success = false, message = "Email hoặc mật khẩu không đúng" });

            if (customer.IsLocked)
                return Json(new { success = false, message = "Tài khoản của bạn đã bị khóa" });

            // Đăng nhập thành công
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, customer.CustomerName),
                new Claim(ClaimTypes.Email, customer.Email),
                new Claim("CustomerID", customer.CustomerID.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.Session.SetString("CustomerEmail", customer.Email);
            HttpContext.Session.SetString("CustomerName", customer.CustomerName);
            HttpContext.Session.SetInt32("CustomerID", customer.CustomerID);

            return Json(new { success = true, name = customer.CustomerName });
        }

        [HttpPost]
        public async Task<IActionResult> AjaxRegister(string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
                return Json(new { success = false, message = "Mật khẩu xác nhận không khớp" });

            if (string.IsNullOrWhiteSpace(email))
                return Json(new { success = false, message = "Vui lòng điền email" });

            if (!await _customerRepository.ValidateEmailAsync(email))
                return Json(new { success = false, message = "Email này đã được sử dụng" });

            var customer = new Customer
            {
                Email = email,
                Password = CryptHelper.HashMD5(password),
                CustomerName = email.Split('@')[0],
                ContactName = email.Split('@')[0],
                IsLocked = false
            };

            await _customerRepository.AddAsync(customer);

            return Json(new { success = true, message = "Đăng ký thành công! Hãy đăng nhập." });
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Home", new { auth = "register" });
        }


        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null) return RedirectToAction("Login");
 
            ViewBag.Provinces = await _provinceRepository.ListAsync();
            var customer = await _customerRepository.GetAsync(customerId.Value);
            return View(customer);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(Customer data)
        {
            if (!ModelState.IsValid) return View(data);

            var customer = await _customerRepository.GetAsync(data.CustomerID);
            if (customer == null) return NotFound();

            customer.CustomerName = data.CustomerName;
            customer.ContactName = data.ContactName;
            customer.Province = data.Province;
            customer.Address = data.Address;
            customer.Phone = data.Phone;
            
            await _customerRepository.UpdateAsync(customer);
            
            HttpContext.Session.SetString("CustomerName", customer.CustomerName);
            ViewBag.Message = "Cập nhật thông tin thành công!";
            
            return View(customer);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp");
                return View();
            }

            int? customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null) return RedirectToAction("Login");

            var customer = await _customerRepository.GetAsync(customerId.Value);
            if (customer == null) return NotFound();

            if (customer.Password != CryptHelper.HashMD5(oldPassword))
            {
                ModelState.AddModelError("oldPassword", "Mật khẩu cũ không đúng");
                return View();
            }

            customer.Password = CryptHelper.HashMD5(newPassword);
            await _customerRepository.UpdateAsync(customer);

            ViewBag.Message = "Đổi mật khẩu thành công!";
            return View();
        }
    }
}
