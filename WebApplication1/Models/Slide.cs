using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;

namespace WebApplication1.Models
{
    public class Slide : BaseEntity
    {

        [MaxLength(50,ErrorMessage= "The value cannot be greater than 50.")]
        [MinLength(2)]
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string? Image { get; set; }
        public int Order { get; set; }

        [NotMapped]
        public IFormFile? Photo {  get; set; }
    }
}
