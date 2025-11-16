using Microsoft.AspNetCore.Mvc;
using WebApplication1.DAL;
using WebApplication1.Models;

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
            if (id == null || id <1)
            {
                return BadRequest();
            }
            Product? product = _context.Products.FirstOrDefault(p=>p.Id==id);
            if (product == null)
            {
                return NotFound();
            }


            return View();
        }
    }
}
