using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.ViewModels.Slides
{
    public class CreateSlideVM
    {

        [MaxLength(50, ErrorMessage = "The value cannot be greater than 50.")]
        [MinLength(2)]
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
  
        public int Order { get; set; }

        public IFormFile Photo { get; set; }
    }
}
