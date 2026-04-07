using Microsoft.AspNetCore.Mvc;

namespace SV22T1020673.Admin.Controllers
{
    public class ItemController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
