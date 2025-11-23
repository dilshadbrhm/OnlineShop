using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Category
{
    public class Category:BaseEntity
    {
        public string Name { get; set; }    
        public List<Product> Products { get; set; }
    }
}
