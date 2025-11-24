using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlideController : Controller
    {
        private readonly AppDBContext _context;

        public SlideController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Slide> slides= await _context.Slides.ToListAsync();
            return View(slides);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Slide slide)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (slide.Order <= 0)
            {
                ModelState.AddModelError(nameof(Slide.Order), "Order must be a positive number.");
                return View(slide);
            }

            bool result = await _context.Slides.AnyAsync(s=>s.Order == slide.Order);
            if(result)
            {
                ModelState.AddModelError(nameof(Slide.Order), $"{slide.Order} order already exist");
                return View();
            }
            
            slide.CreatedAt = DateTime.Now;
            _context.Add(slide);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            
            
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id<1)
            {
                return BadRequest();
            }
            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if(existed == null)
            {
                return NotFound();
            }
            return View(existed);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, Slide slide)
        {
            if (id == null || id < 1)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(slide);

            bool result = await _context.Slides
                .AnyAsync(s => s.Order == slide.Order && s.Id != id);

            if (result)
            {
                ModelState.AddModelError(nameof(Slide.Order), $"{slide.Order} order already exist");
                return View(slide);
            }

            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed == null)
                return NotFound();

            existed.Title = slide.Title;
            existed.Subtitle = slide.Subtitle;
            existed.Order = slide.Order;
            existed.Image = slide.Image;
            existed.Description = slide.Description;
           

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
