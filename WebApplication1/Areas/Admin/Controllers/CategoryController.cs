using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDBContext _context;

        public CategoryController(AppDBContext context)
        {
            _context = context;
        }

   
        public async Task<IActionResult> Index()
        {
            
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);   
            }

            bool existed = await _context.Categories.AnyAsync(c => c.Name == category.Name);

            if (existed)
            {
                ModelState.AddModelError(nameof(Category.Name), "Category name already exists.");
                return View(category);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1)
                return BadRequest();

            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            return View(category);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, Category category)
        {
            if (id == null || id < 1) return BadRequest();

            if (id != category.Id) return BadRequest();

            if (!ModelState.IsValid) return View(category);

            
            bool existed = await _context.Categories
                .AnyAsync(c => c.Name == category.Name && c.Id != category.Id);

            if (existed)
            {
                ModelState.AddModelError(nameof(Category.Name),"Category name already exists.");
                return View(category);
            }

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id <= 0) return BadRequest();

            Category? category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category is null) return NotFound();

            

            _context.Categories.Remove(category);


            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
           
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Category? category = await _context.Categories
                .Include(c => c.Products)
                    .ThenInclude(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                .Include(c => c.Products)
                    .ThenInclude(p => p.ProductSizes)
                        .ThenInclude(ps => ps.Size)
                .Include(c => c.Products)
                    .ThenInclude(p => p.ProductColors)
                        .ThenInclude(pc => pc.Color)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }
    }
}
