using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.Utilities.Enums;
using WebApplication1.Utilities.Extensions;
using WebApplication1.ViewModels;
using WebApplication1.ViewModels.Products;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class ProductController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index(int page = 1)
        {

            int totalCount = await _context.Products.CountAsync();

           

            int total = (int)Math.Ceiling((decimal)totalCount / 3);
            
            if (page < 1 || page > total)
            {
                return BadRequest();
            }


            var productVMs = await _context.Products
                .Skip((page - 1) * 3)
                .Take(3)
                .Select(p => new GetAdminProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Image = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                    CategoryName = p.Category.Name
                }).ToListAsync();

            PaginatedItemsVM<GetAdminProductVM> paginatedVM = new()
            {
                Items = productVMs,
                CurrentPage = page,
                TotalPage = total
            };
            return View(paginatedVM);
        }
        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM()
            {
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync()

            };
            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();
            if (!ModelState.IsValid)
            {

                return View(productVM);
            }
            if (!productVM.PrimaryPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateProductVM.PrimaryPhoto), "File type is incorrect.");
                return View(productVM);
            }
            if (!productVM.PrimaryPhoto.ValidateSize(Utilities.Enums.FileSize.MB, 1))
            {
                ModelState.AddModelError(nameof(CreateProductVM.PrimaryPhoto), "File size is incorrect.");
                return View(productVM);
            }


            if (!productVM.SecondaryPhoto.ValidateType("image/"))
            {
                ModelState.AddModelError(nameof(CreateProductVM.SecondaryPhoto), "File type is incorrect.");
                return View(productVM);
            }
            if (!productVM.SecondaryPhoto.ValidateSize(Utilities.Enums.FileSize.MB, 1))
            {
                ModelState.AddModelError(nameof(CreateProductVM.SecondaryPhoto), "File size is incorrect.");
                return View(productVM);
            }

            bool result = _context.Categories.Any(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(CreateProductVM.CategoryId), "Category does not exist.");
                return View(productVM);
            }

            if (productVM.TagIds == null) productVM.TagIds = new();

            productVM.TagIds = productVM.TagIds.Distinct().ToList();

            bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));
            if (tagResult)
            {
                ModelState.AddModelError(nameof(CreateProductVM.TagIds), "Tags are wrong");
                return View(productVM);
            }
            bool resultName = await _context.Products.AnyAsync(p => p.Name == productVM.Name);
            if (resultName)
            {
                ModelState.AddModelError(nameof(CreateProductVM.Name), "Product Name alreay  exist.");
                return View(productVM);
            }
            if (productVM.Price < 0)
            {
                ModelState.AddModelError(nameof(CreateProductVM.Price), "Price cannot be zero or negative.");
                return View(productVM);
            }

            ProductImage main = new()
            {
                Image = await productVM.PrimaryPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                IsPrimary = true,
                CreatedAt = DateTime.Now

            };
            ProductImage secondary = new()
            {
                Image = await productVM.SecondaryPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                IsPrimary = false,
                CreatedAt = DateTime.Now

            };

            Product product = new()
            {

                Name = productVM.Name,
                SKU = productVM.SKU,
                Price = productVM.Price.Value,
                Description = productVM.Description,
                CategoryId = productVM.CategoryId.Value,
                CreatedAt = DateTime.Now,
                ProductTags = productVM.TagIds.Select(tId => new ProductTag { TagId = tId }).ToList(),
                ProductImages = new() { main, secondary }
            };

            string message = string.Empty;

            if (productVM.AdditionalPhotos != null)
            {
                foreach (IFormFile file in productVM.AdditionalPhotos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        message += $" <p class=\"text-warning\">{file.FileName} file type is incorrect.</p>";
                        continue;
                    }
                    if (!file.ValidateSize(FileSize.MB, 1))
                    {
                        message += $" <p class=\"text-warning\">{file.FileName} file size is incorrect.</p>";
                        continue;
                    }

                    product.ProductImages.Add(new()
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                        IsPrimary = null,
                        CreatedAt = DateTime.Now
                    });
                }
                TempData["ImageWarning"] = message;
            }


            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [Authorize]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {

            if (id == null || id < 0)
            {
                return BadRequest();
            }
            Product? existed = await _context.Products
                .Include(p => p.ProductTags)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (existed == null)
            {
                return NotFound();
            }
            UpdateProductVM productVM = new()
            {
                Name = existed.Name,
                SKU = existed.SKU,
                Description = existed.Description,
                CategoryId = existed.CategoryId,
                Price = existed.Price,
                TagIds = existed.ProductTags.Select(pt => pt.TagId).ToList(),
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                ProductImages = existed.ProductImages
            };

            return View(productVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM productVM)
        {
            Product? existed = await _context.Products
               .Include(p => p.ProductImages)
               .Include(p => p.ProductTags)
               .FirstOrDefaultAsync(p => p.Id == id);

            productVM.Categories = await _context.Categories.ToListAsync();
            productVM.Tags = await _context.Tags.ToListAsync();
            productVM.ProductImages = existed.ProductImages;

            if (!ModelState.IsValid)
            {
                return View(productVM);
            }
            if (productVM.PrimaryPhoto != null)
            {
                if (!productVM.PrimaryPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.PrimaryPhoto), "File type is incorrect.");
                    return View(productVM);
                }
                if (!productVM.PrimaryPhoto.ValidateSize(Utilities.Enums.FileSize.MB, 1))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.PrimaryPhoto), "File size is incorrect.");
                    return View(productVM);
                }
            }
            if (productVM.SecondaryPhoto != null)
            {
                if (!productVM.SecondaryPhoto.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.SecondaryPhoto), "File type is incorrect.");
                    return View(productVM);
                }
                if (!productVM.SecondaryPhoto.ValidateSize(Utilities.Enums.FileSize.MB, 1))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.SecondaryPhoto), "File size is incorrect.");
                    return View(productVM);
                }
            }



            bool result = _context.Categories.Any(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.CategoryId), "Category does not exist.");
                return View(productVM);
            }

            if (productVM.TagIds == null) productVM.TagIds = new();

            productVM.TagIds = productVM.TagIds.Distinct().ToList();

            bool tagResult = productVM.TagIds.Any(tId => !productVM.Tags.Exists(t => t.Id == tId));
            if (tagResult)
            {
                ModelState.AddModelError(nameof(CreateProductVM.TagIds), "Tags are wrong");
                return View(productVM);
            }
            bool resultName = await _context.Products.AnyAsync(p => p.Name == productVM.Name && p.Id != id);
            if (resultName)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.Name), "Product Name alreay  exist.");
                return View(productVM);
            }
            if (productVM.Price < 0)
            {
                ModelState.AddModelError(nameof(UpdateProductVM.Price), "Price cannot be zero or negative.");
                return View(productVM);
            }


            if (productVM.PrimaryPhoto != null)
            {
                string mainFileName = await productVM.PrimaryPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage existedMain = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true);
                existedMain.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");

                existed.ProductImages.Remove(existedMain);


                existed.ProductImages.Add(new()
                {
                    Image = mainFileName,
                    IsPrimary = true,
                    CreatedAt = DateTime.Now
                });
            }
            if (productVM.SecondaryPhoto != null)
            {
                string secondaryName = await productVM.SecondaryPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage existedSecondary = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false);
                existedSecondary.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");

                existed.ProductImages.Remove(existedSecondary);


                existed.ProductImages.Add(new()
                {
                    Image = secondaryName,
                    IsPrimary = false,
                    CreatedAt = DateTime.Now
                });
            }



            if (productVM.ImageIds == null)
            {
                productVM.ImageIds = new();
            }



            List<ProductImage> deletedImage = existed.ProductImages
                .Where(pi => !productVM.ImageIds
                    .Exists(imgId => pi.Id == imgId) && pi.IsPrimary == null)
                .ToList();

            deletedImage
                .ForEach(di => di.Image
                    .DeleteFile(_env.WebRootPath, "assets", "images", "website-images"));

            _context.ProductImages
                .RemoveRange(deletedImage);


            _context.ProductTags
                .RemoveRange(existed.ProductTags
                    .Where(pt => productVM.TagIds
                    .Exists(tId => pt.TagId == tId))
                    .ToList());





            _context.ProductTags
                .AddRange(productVM.TagIds
                    .Where(tId => existed.ProductTags
                        .Exists(pt => pt.TagId == tId))
                    .Select(tId => new ProductTag { TagId = tId, ProductId = existed.Id })
                    .ToList());


            if (productVM.AdditionalPhotos != null)
            {
                string message = string.Empty;
                foreach (IFormFile file in productVM.AdditionalPhotos)
                {
                    if (!file.ValidateSize(FileSize.MB, 1))
                    {
                        message += $" <p class=\"text-warning\">{file.FileName} file type is incorrect.</p>";
                        continue;
                    }
                    if (!file.ValidateType("image/"))
                    {
                        message += $"<p>{file.FileName} named image type is incorrect</p>";
                        continue;
                    }

                    existed.ProductImages.Add(new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images"),
                        IsPrimary = null,
                        CreatedAt = DateTime.Now

                    });

                }

                TempData["FileWarning"] = message;
            }

            existed.Name = productVM.Name;
            existed.SKU = productVM.SKU;
            existed.Description = productVM.Description;
            existed.Price = productVM.Price.Value;
            existed.CategoryId = productVM.CategoryId.Value;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id < 1) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            DetailsProductVM productVM = new DetailsProductVM
            {

                Name = product.Name,
                Price = product.Price,
                SKU = product.SKU,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                TagIds = product.ProductTags.Select(pt => pt.TagId).ToList(),
                ProductImages = product.ProductImages
            };

            return View(productVM);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null || id < 1) return BadRequest();

            Product? product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            product.ProductImages.ForEach(pi => pi.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images"));


            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
