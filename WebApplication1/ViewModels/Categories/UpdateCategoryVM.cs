using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Categories
{
    public class UpdateCategoryVM
    {
        [Required]
        public string Name { get; set; }
    }
}
