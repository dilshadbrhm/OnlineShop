namespace WebApplication1.Models
{
    public class Category:BaseEntity
    {
        public string Name { get; set; }
        //retational
        public List<Product>? Products { get; set; }
    }
}
