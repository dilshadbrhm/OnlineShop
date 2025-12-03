using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class GetAdminCategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
   
        public string CategoryName { get; set; }

        public string Image { get; set; }
    }
}
