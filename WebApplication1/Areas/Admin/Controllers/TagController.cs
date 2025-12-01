using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.DAL;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TagController : Controller
    {
        private readonly AppDBContext _context;

        public TagController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var tags = await _context.Tags.Include(p => p.ProductTags).ToListAsync();
            return View(tags);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Tag tag)
        {
            if (!ModelState.IsValid)
                return View(tag);

            bool existed = await _context.Tags.AnyAsync(t => t.Name == tag.Name);

            if (existed)
            {
                ModelState.AddModelError(nameof(Tag.Name), "Tag name already exists.");
                return View(tag);
            }

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }
                

            var tag = await _context.Tags.Include(t => t.ProductTags).FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return View(tag);
            }

            Tag? existed = await _context.Tags.FindAsync(id);
            if (existed == null)
            {
                return NotFound();
            }

            bool nameResult = await _context.Tags.AnyAsync(t => t.Name.Trim().ToLower() == tag.Name && t.Id != id);

            if (nameResult)
            {
                ModelState.AddModelError(nameof(Tag.Name), "Tag name already exists.");
                return View(tag);
            }

            existed.Name = tag.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
