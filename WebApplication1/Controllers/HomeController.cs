using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;

using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDBContext _context;

        private HomeVM homeVM;

        public HomeController(AppDBContext context)
        {
            _context = context;

        }
        public async Task<IActionResult> Index()
        {


            HomeVM homeVM = new HomeVM
            {
                Slides = await _context
                .Slides
                .OrderBy(s => s.Order)
                .Take(2)
                .ToListAsync(),

                Products = await _context.Products
                .OrderBy(p => p.CreatedAt)
                .Take(8)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .ToListAsync()
            };
            return View(homeVM);




        }
        public IActionResult Contact()
        {
            return View();
        }

    }
}
