using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels.Products
{
    public class DetailsProductVM
    {
        public string Name { get; set; }
        [Required]
        [Range(0.01D, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal? Price { get; set; }
        public string SKU { get; set; }
        public string Description { get; set; }
        public string PrimaryPhoto { get; set; }
        public string SecondaryPhoto { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        public List<Category>? Categories { get; set; }
        public List<int>? TagIds { get; set; }
        public List<Tag>? Tags { get; set; }
        public List<int>? SizeIds { get; set; }
        public List<Size>? Sizes { get; set; }
        public List<int>? ColorIds { get; set; }
        public List<Color>? Colors { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
    }
}
