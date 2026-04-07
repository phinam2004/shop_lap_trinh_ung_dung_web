using Microsoft.AspNetCore.Mvc;
using SV22T1020673.Shop.Models;
using System.Diagnostics;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Catalog;
using SV22T1020673.Models.Common;

namespace SV22T1020673.Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly IGenericRepository<Category> _categoryRepository;

        public HomeController(ILogger<HomeController> logger, 
                            IProductRepository productRepository,
                            IGenericRepository<Category> categoryRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var categoryResult = await _categoryRepository.ListAsync(new PaginationSearchInput
            {
                Page = 1,
                PageSize = 20, // Fetch up to 20 categories for the sidebar
                SearchValue = ""
            });

            var productResult = await _productRepository.ListAsync(new ProductSearchInput
            {
                Page = 1,
                PageSize = 12,
                SearchValue = ""
            });

            var viewModel = new HomeViewModel
            {
                Categories = categoryResult.DataItems,
                Products = productResult.DataItems
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
