using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketController(AppDBContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            BasketVM basketVM = new BasketVM()
            {
                BasketItemVMs = new List<BasketItemVM>()
            };
            if (User.Identity.IsAuthenticated)
            {
                basketVM.BasketItemVMs= await _context.BasketItems
                    .Where(bi=>bi.AppUserId==User.FindFirstValue(ClaimTypes.NameIdentifier))
                    .Select(bi=>new BasketItemVM { 
                            Count = bi.Count,
                            Name=bi.Product.Name,
                            Price=bi.Product.Price,
                            Image=bi.Product.ProductImages.FirstOrDefault(pi=>pi.IsPrimary==true).Image,
                            SubTotal=bi.Count*bi.Product.Price
                    
                    }).ToListAsync();

                basketVM.BasketItemVMs.ForEach(b => basketVM.Total += b.SubTotal);
            }
            else
            {
                string json = Request.Cookies["Basket"];

                List<BasketCookieItemVM> items;
               


                if (json != null)
                {
                    items = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(json);
                }
                else
                {
                    items = new();
                }
                foreach (BasketCookieItemVM cookie in items)
                {
                    Product? product = await _context.Products
                        .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                        .FirstOrDefaultAsync(p => cookie.ProductId == p.Id);
                    if (product != null)
                    {

                        basketVM.BasketItemVMs.Add(new BasketItemVM
                        {
                            ProductId = product.Id,
                            Name = product.Name,
                            Price = product.Price,
                            Image = product.ProductImages[0].Image,
                            Count = cookie.Count,
                            SubTotal = cookie.Count * product.Price

                        });
                        basketVM.Total += cookie.Count * product.Price;
                    }
                }
            }
              
            return View(basketVM);
        }
        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null || id < 1) return BadRequest();


            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            if (User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users
                      .Include(u => u.BasketItems)
                      .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                BasketItem item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
                if(item == null)
                {
                    user.BasketItems.Add(new BasketItem
                    {
                        Count = 1,
                        ProductId = id.Value,
                        CreatedAt = DateTime.Now
                    });
                }
                else
                {
                    item.Count++;
                }

                await _context.SaveChangesAsync();

                //_context.BasketItems.Where(bi => bi.AppUserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            }
            else
            {
                List<BasketCookieItemVM> items;
                if (Request.Cookies["basket"] == null)
                {
                    items = new List<BasketCookieItemVM>();
                    items.Add(new BasketCookieItemVM
                    {
                        ProductId = product.Id,
                        Count = 1
                    });
                }
                else
                {
                    string str = Request.Cookies["basket"];
                    items = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(str);
                    BasketCookieItemVM itemsVM = items.FirstOrDefault(i => i.ProductId == id);
                    if (itemsVM != null)
                    {
                        itemsVM = new BasketCookieItemVM { ProductId = id.Value, Count = 1 };
                        items.Add(itemsVM);
                    }
                    else
                    {
                        itemsVM.Count++;
                    }
                }
                string json = JsonConvert.SerializeObject(items);

                Response.Cookies.Append("Basket", json);

            }



            return RedirectToAction("Index", "Home");
        }
        public IActionResult GetAction()
        {
            return Content(Request.Cookies["Basket"]);
        }
    }
}
