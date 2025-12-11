using Microsoft.AspNetCore.Identity;
using WebApplication1.Models.Enums;

namespace WebApplication1.Models
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
        public string Surname {  get; set; } 
        
        public DateTime Birthday { get; set; }
        public Gender Gender { get; set; }
        public string ProfileImage { get; set; }

        public List<BasketItem> BasketItems { get; set; }

    }
}
