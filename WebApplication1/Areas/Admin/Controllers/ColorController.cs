using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ColorController : Controller
    {
        private readonly AppDBContext _context;
        public ColorController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var colors = await _context.Colors
              .Include(c => c.ProductColors)
              .ThenInclude(pc => pc.Product)
              .ToListAsync();

            return View(colors);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Color color)
        {
            if (!ModelState.IsValid)
            {
                return View(color);
            }
               

            bool existed = await _context.Colors.AnyAsync(c => c.Name == color.Name);

            if (existed)
            {
                ModelState.AddModelError(nameof(Color.Name), "Color name already exists.");
                return View(color);
            }

            _context.Colors.Add(color);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }
               

            var color = await _context.Colors.Include(c => c.ProductColors).FirstOrDefaultAsync(c => c.Id == id);

            if (color == null)
            {
                return NotFound();
            }
              

            return View(color);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Color color)
        {
            if (!ModelState.IsValid)
            {
                return View(color);
            }
                

            var existed = await _context.Colors.FindAsync(id);
            if (existed == null)
            {
                return NotFound();
            }
                

            bool nameExists = await _context.Colors.AnyAsync(c => c.Name == color.Name && c.Id != id);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(Color.Name), "Color name already exists.");
                return View(color);
            }

            existed.Name = color.Name.Trim();

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

