using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.Utilities.Enums;
using WebApplication1.ViewModels;
using WebApplication1.ViewModels.Categories;
using WebApplication1.ViewModels.Products;

namespace WebApplication1.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDBContext _context;

        public ShopController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? search, int? categoryId, int key = 1, int page = 1)
        {

            IQueryable<Product> query = _context.Products;

            if (search != null)
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower().Trim()));
            }
            if (categoryId != null)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            switch (key)
            {
                case (int)SortType.Name:
                    query = query.OrderBy(p => p.Name);
                    break;
                case (int)SortType.Price:
                    query = query.OrderBy(p => p.Price);
                    break;
                case (int)SortType.Date:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            int totalCount = await query.CountAsync();
            int totalPage =(int)Math.Ceiling((double)totalCount / 2);
            query = query.Skip((page - 1) * 2).Take(2);

            ShopVM shopVM = new()
            {
                ProductVMs = await query.Select(p => new GetProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    PrimaryImage = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                    SecondaryImage = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false).Image,

                }).ToListAsync(),
                CategoryVMs = await _context.Categories.Select(c => new GetCategoryVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Count = c.Products.Count()
                }).ToListAsync(),
                Search = search,
                CategoryId = categoryId,
                Key = key,
                TotalPage = totalPage,
                CuurentPage = page
            };



            return View(shopVM);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }
            Product? product = await _context.Products
                .Include(p => p.ProductImages.OrderByDescending(pi => pi.IsPrimary))
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            List<Product> relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .ToListAsync();

            DetailsVM detailsVM = new DetailsVM()
            {
                Product = product,
                RelatedProducts = relatedProducts
            };
            return View(detailsVM);
        }
    }
}
