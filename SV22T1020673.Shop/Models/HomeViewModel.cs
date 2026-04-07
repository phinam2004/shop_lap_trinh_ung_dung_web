using SV22T1020673.Models.Catalog;

namespace SV22T1020673.Shop.Models
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
