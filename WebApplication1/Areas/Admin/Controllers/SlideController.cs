using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.Utilities.Extensions;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlideController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDBContext context,IWebHostEnvironment env)
        {
            _context = context;
            _env = env;

            
        }
        public async Task<IActionResult> Index()
        {
            List<Slide> slides = await _context.Slides.AsNoTracking().ToListAsync();
            return View(slides);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Slide slide)
        {
            ModelState.Remove(nameof(Slide.Image));
            ModelState.Remove(nameof(Slide.CreatedAt));
            if (slide.Photo == null)
            {
                ModelState.AddModelError(nameof(slide.Photo), "Please choose an image.");
                return View(slide);
            }
            if (!slide.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(slide.Photo), "File type is incorrect.");
                return View();
            }
            if (!slide.Photo.ValidateSize(Utilities.Enums.FileSize.MB,2))
            {
                ModelState.AddModelError(nameof(slide.Photo), "File size is incorrect.");
                return View();
            }
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (slide.Order <= 0)
            {
                ModelState.AddModelError(nameof(Slide.Order), "Order must be a positive number.");
                return View(slide);
            }



            bool result = await _context.Slides.AnyAsync(s => s.Order == slide.Order);
            if (result)
            {
                ModelState.AddModelError(nameof(Slide.Order), $"{slide.Order} order already exist.");
                return View();
            }

            string fileName=string.Concat(Guid.NewGuid() , Path.GetExtension(slide.Photo.FileName));
            string path = Path.Combine(_env.WebRootPath, "assets","images","website-images",fileName);
          
            FileStream stream = new FileStream(path,FileMode.Create);

            await slide.Photo.CopyToAsync(stream);
            slide.Image = fileName;

            slide.CreatedAt = DateTime.Now;
            _context.Add(slide);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }
            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed == null)
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
