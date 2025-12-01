using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SizeController : Controller
    {
        private readonly AppDBContext _context;

        public SizeController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var sizes = await _context.Sizes.Include(s => s.ProductSizes).ThenInclude(ps => ps.Product).ToListAsync();

            return View(sizes);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Size size)
        {
            if (!ModelState.IsValid)
            {
                return View(size);
            }
               

            bool existed = await _context.Sizes.AnyAsync(s => s.Name== size.Name);

            if (existed)
            {
                ModelState.AddModelError(nameof(Size.Name), "Size already exists.");
                return View(size);
            }

            _context.Sizes.Add(size);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }
                

            var size = await _context.Sizes.Include(s => s.ProductSizes).FirstOrDefaultAsync(s => s.Id == id);

            if (size == null)
            {
                return NotFound();
            }
                

            return View(size);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Size size)
        {
            if (!ModelState.IsValid)
            {
                return View(size);
            }
               

            var existed = await _context.Sizes.FirstOrDefaultAsync(s => s.Id == id); 
            if (existed == null)
            {
                return NotFound();
            }
                

            bool nameExists = await _context.Sizes.AnyAsync(s => s.Name == size.Name && s.Id != id);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(Size.Name), "Size already exists.");
                return View(size);
            }

            existed.Name = size.Name.Trim();

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

