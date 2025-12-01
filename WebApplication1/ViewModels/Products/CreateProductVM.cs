using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels.Products
{
    public class CreateProductVM
    {
        public string Name { get; set; }
        [Required]
        public decimal? Price { get; set; }
        public string SKU { get; set; }
        public string Description { get; set; }

        [Required]
        public int? CategoryId { get; set; }
        public List<Category>? Categories { get; set;} 

    }
}
