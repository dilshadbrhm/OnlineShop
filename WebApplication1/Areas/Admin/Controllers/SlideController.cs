using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.Utilities.Enums;
using WebApplication1.Utilities.Extensions;
using WebApplication1.ViewModels.Slides;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlideController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;


        }
        public async Task<IActionResult> Index()
        {
            List<GetSlideVM> slideVMs = await _context.Slides.AsNoTracking()
                .Select(s=>new GetSlideVM { 
                    Id = s.Id,
                    Title = s.Title,
                    Image= s.Image,
                    Order = s.Order
                })
                .ToListAsync();
           

            return View(slideVMs);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateSlideVM slideVM)
        {
           
            if (slideVM.Photo == null)
            {
                ModelState.AddModelError(nameof(CreateSlideVM.Photo), "Please choose an image.");
                return View(slideVM);
            }
            if (!slideVM.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateSlideVM.Photo), "File type is incorrect.");
                return View();
            }
            if (!slideVM.Photo.ValidateSize(Utilities.Enums.FileSize.MB, 2))
            {
                ModelState.AddModelError(nameof(CreateSlideVM.Photo), "File size is incorrect.");
                return View();
            }
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (slideVM.Order <= 0)
            {
                ModelState.AddModelError(nameof(CreateSlideVM.Order), "Order must be a positive number.");
                return View(slideVM);
            }



            bool result = await _context.Slides.AnyAsync(s => s.Order == slideVM.Order);
            if (result)
            {
                ModelState.AddModelError(nameof(Slide.Order), $"{slideVM.Order} order already exist.");
                return View();
            }



            string fileName = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");

            Slide slide = new Slide()
            {
                Title = slideVM.Title,
                Subtitle = slideVM.Subtitle,
                Order = slideVM.Order,
                Description = slideVM.Description,
                Image = fileName,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };


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
            UpdateSlideVM slideVM = new UpdateSlideVM
            {
                Title = existed.Title,
                Subtitle = existed.Subtitle,
                Order = existed.Order,
                Description = existed.Description,
                Image = existed.Image
            };
            return View(slideVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateSlideVM slideVM)
        {
            if (id == null || id < 1)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(slideVM);
            }
            if (slideVM != null)
            {
                if (!slideVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "File type is incorrect.");
                    return View(slideVM);
                }
                if (!slideVM.Photo.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError(nameof(UpdateSlideVM.Photo), "File type is incorrect.");
                    return View(slideVM);
                }

            }

            bool result = await _context.Slides
                .AnyAsync(s => s.Order == slideVM.Order && s.Id != id);

            if (result)
            {
                ModelState.AddModelError(nameof(UpdateSlideVM.Order), $"{slideVM.Order} order already exist.");
                return View(slideVM);
            }

            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed == null)
            {
                return NotFound();
            }

            if (slideVM.Photo != null)
            {
              
                string fileName = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                existed.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.Image=fileName;
            }

            existed.Title = slideVM.Title;
            existed.Subtitle = slideVM.Subtitle;
            existed.Order = slideVM.Order;

            existed.Description = slideVM.Description;




            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async  Task<IActionResult> Delete(int? id)
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

            existed.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
            _context.Remove(existed);
            //existed.IsDeleted = true;
            await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
        }
    }
}
