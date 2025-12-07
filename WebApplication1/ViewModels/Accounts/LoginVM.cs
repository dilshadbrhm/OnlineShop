using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class LoginVM
    {
        [MaxLength(128)]
        [MinLength(4)]
        public string UserNameorEmail { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool IsPersistant { get; set; }
    }
}
