using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Colors
{
    public class CreateColorVM
    {
        [Required]
        public string Name { get; set; }
    }
}
