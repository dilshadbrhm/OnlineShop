using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Slides
{
    public class UpdateSlideVM
    {
        [MaxLength(50, ErrorMessage = "The value cannot be greater than 50.")]
        [MinLength(2)]
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Order { get; set; }
        public IFormFile? Photo { get; set; }
    }
}
