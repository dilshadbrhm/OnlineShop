using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class CreateSizeVM
    {
        [Required]
        public string Name { get; set; }
    }
}
