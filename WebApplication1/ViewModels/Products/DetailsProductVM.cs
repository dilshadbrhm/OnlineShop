using WebApplication1.Models;

namespace WebApplication1.ViewModels.Products
{
    public class DetailsProductVM
    {
        public Product Product { get; set; }
        public List<Category> Categories { get; set; }
    }
}
