using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDBContext _context;

        public ShopController(AppDBContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Details(int? id)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }
            Product? product = _context.Products
                .Include(p => p.ProductImages.OrderByDescending(pi=>pi.IsPrimary))
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            List<Product> relatedProducts = _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .ToList();

            DetailsVM detailsVM = new DetailsVM()
            {
                Product = product,
                RelatedProducts=relatedProducts
            };
            return View(detailsVM);
        }
    }
}
