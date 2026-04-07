using Microsoft.AspNetCore.Mvc;
using SV22T1020673.Admin;
using SV22T1020673.BusinessLayers;
using SV22T1020673.Models.Common;
using SV22T1020673.Models.HR;

namespace SV22T1020673.Admin.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private const int PAGESIZE = 10;
        private const string EMPLOYEE_SEARCH_INPUT = "EmployeeSearchInput";

        public EmployeeController(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(EMPLOYEE_SEARCH_INPUT);
            if (input == null)
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = ""
                };

            return View(input);
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await HRDataService.ListEmployeesAsync(input);
            ApplicationContext.SetSessionData(EMPLOYEE_SEARCH_INPUT, input);
            return View(result);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            var model = new Employee()
            {
                EmployeeID = 0,
                IsWorking = true
            };
            return View("Edit", model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SaveData(Employee data, IFormFile? uploadPhoto)
        {
            ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";

            // Xử lý ảnh tải lên (nếu có)
            if (uploadPhoto != null)
            {
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                string folder = Path.Combine(_hostEnvironment.WebRootPath, "images", "employees");
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                data.Photo = fileName;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(data.FullName))
                    ModelState.AddModelError(nameof(data.FullName), "Vui lòng nhập họ tên nhân viên");
                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email");
                else if (!(await HRDataService.ValidateEmployeeEmailAsync(data.Email, data.EmployeeID)))
                    ModelState.AddModelError(nameof(data.Email), "Email này đã có người sử dụng");

                if (string.IsNullOrEmpty(data.Address))
                    data.Address = "";
                if (string.IsNullOrEmpty(data.Phone))
                    data.Phone = "";

                if (data.EmployeeID > 0 && string.IsNullOrWhiteSpace(data.Photo))
                {
                    var oldData = await HRDataService.GetEmployeeAsync(data.EmployeeID);
                    data.Photo = oldData?.Photo;
                }

                if (!ModelState.IsValid)
                    return View("Edit", data);

                if (data.EmployeeID == 0)
                {
                    data.Password = CryptHelper.HashMD5("123456"); // Mật khẩu mặc định cho nhân viên mới
                    await HRDataService.AddEmployeeAsync(data);
                }
                else
                    await HRDataService.UpdateEmployeeAsync(data);

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("Error", "Hệ thống đang bận, Vui lòng thử lại sau!");
                return View("Edit", data);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await HRDataService.DeleteEmployeeAsync(id);
                return RedirectToAction("Index");
            }

            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete = !(await HRDataService.IsUsedEmployeeAsync(id));
            return View(model);
        }
        public async Task<IActionResult> ChangePassword(int id)
        {
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                ModelState.AddModelError("newPassword", "Vui lòng nhập mật khẩu mới");
            if (newPassword != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp");

            if (!ModelState.IsValid)
            {
                var model = await HRDataService.GetEmployeeAsync(id);
                return View(model);
            }

            string passwordHash = CryptHelper.HashMD5(newPassword);
            await HRDataService.UpdateEmployeePasswordAsync(id, passwordHash);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ChangeRole(int id)
        {
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(int id, string[] roles)
        {
            string roleNames = roles != null ? string.Join(",", roles) : "";
            await HRDataService.UpdateEmployeeRoleNamesAsync(id, roleNames);

            return RedirectToAction("Index");
        }
    }
}
