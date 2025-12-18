using WebApplication1.Models;

namespace WebApplication1.ViewModels.Products
{
    public class GetProductVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public string PrimaryImage { get; set; }

        public string SecondaryImage { get; set; }
    }
}
