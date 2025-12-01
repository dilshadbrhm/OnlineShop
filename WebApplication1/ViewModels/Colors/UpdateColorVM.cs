using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Colors
{
    public class UpdateColorVM
    {
        [Required]
        public string Name { get; set; }
    }
}
