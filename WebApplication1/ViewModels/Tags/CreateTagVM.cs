using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Tags
{
    public class CreateTagrVM
    {
        [Required]
        public string Name { get; set; }
    }
}
