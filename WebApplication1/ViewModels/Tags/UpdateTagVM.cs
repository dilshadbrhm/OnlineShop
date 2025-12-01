using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Tags
{
    public class UpdateTagVM
    {
        [Required]
        public string Name { get; set; }
    }
}
