using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Categories
{
    public class CreateCategoryVM
    {
        [Required]
        public string Name { get; set; }
    }
}
