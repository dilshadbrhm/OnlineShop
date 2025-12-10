using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Tasks;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDBContext _context;

        public BasketController(AppDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            string json = Request.Cookies["Basket"];

            List<BasketCookieItemVM> items;
            BasketVM basketVM = new BasketVM()
            {
                BasketItemVMs = new List<BasketItemVM>()
            };


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
            return View(basketVM);
        }
        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null || id < 1) return BadRequest();


            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();
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

            return RedirectToAction("Index", "Home");
        }
        public IActionResult GetAction()
        {
            return Content(Request.Cookies["Basket"]);
        }
    }
}
