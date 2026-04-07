using Microsoft.AspNetCore.Mvc;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Catalog;
using SV22T1020673.Models.Common;

namespace SV22T1020673.Shop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IGenericRepository<Category> _categoryRepository;

        public ProductController(IProductRepository productRepository, IGenericRepository<Category> categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index(ProductSearchInput input)
        {
            input.PageSize = 12; // 12 items per page for better grid layout
            if (input.Page <= 0) input.Page = 1;

            var result = await _productRepository.ListAsync(input);
            ViewBag.Categories = (await _categoryRepository.ListAsync(new PaginationSearchInput { Page = 1, PageSize = 100 })).DataItems;
            
            return View(result);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _productRepository.GetAsync(id);
            if (product == null) return NotFound();

            ViewBag.Attributes = await _productRepository.ListAttributesAsync(id);
            ViewBag.Photos = await _productRepository.ListPhotosAsync(id);
            ViewBag.Category = await _categoryRepository.GetAsync(product.CategoryID ?? 0);

            // Fetch related products (same category, different ID)
            var relatedInput = new ProductSearchInput
            {
                Page = 1,
                PageSize = 6,
                CategoryID = product.CategoryID ?? 0,
                SearchValue = ""
            };
            var relatedResult = await _productRepository.ListAsync(relatedInput);
            ViewBag.RelatedProducts = relatedResult.DataItems.Where(p => p.ProductID != id).ToList();

            return View(product);
        }
    }
}
