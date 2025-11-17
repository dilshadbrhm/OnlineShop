using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class DetailsVM
    {
        public Product Product { get; set; }
        public List<Product> RelatedProducts {  get; set; }
    }
}
