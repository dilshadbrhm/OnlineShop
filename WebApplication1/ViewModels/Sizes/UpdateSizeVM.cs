using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class UpdateSizeVM
    {
        [Required]
        public string Name { get; set; }
    }
}
