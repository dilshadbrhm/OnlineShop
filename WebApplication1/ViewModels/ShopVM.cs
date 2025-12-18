using WebApplication1.ViewModels.Categories;
using WebApplication1.ViewModels.Products;

namespace WebApplication1.ViewModels
{
    public class ShopVM
    {
        public List<GetProductVM> ProductVMs { get; set; }
        public List<GetCategoryVM> CategoryVMs { get; set; }
        public int? CategoryId { get; set; }
        public string? Search { get; set; }
        public int Key { get; set; }
        public int TotalPage { get; set; }
        public int CuurentPage { get; set; }


    }
}
