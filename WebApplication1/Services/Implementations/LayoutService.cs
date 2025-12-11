using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.Services.Interfaces;
using WebApplication1.ViewModels;

namespace WebApplication1.Services.Implementations
{
    public class LayoutService:ILayoutService
    {
        private readonly AppDBContext _context;
        private readonly HttpContext _httpContext;
        //private readonly IHttpContextAccessor _accessor;

        public LayoutService(AppDBContext context,IHttpContextAccessor accessor)
        {
            _context = context;
            //_accessor = accessor;
            _httpContext = accessor.HttpContext;
        }

     

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);


            return settings;
        }

        public async Task<BasketVM> GetBasketAsync()
        {
            
            BasketVM basketVM = new BasketVM()
            {
                BasketItemVMs = new List<BasketItemVM>()
            };
            if (_httpContext.User.Identity.IsAuthenticated)
            {
                basketVM.BasketItemVMs = await _context.BasketItems
                    .Where(bi => bi.AppUserId == _httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))
                    .Select(bi => new BasketItemVM
                    {
                        Count = bi.Count,
                        Name = bi.Product.Name,
                        Price = bi.Product.Price,
                        Image = bi.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                        SubTotal = bi.Count * bi.Product.Price

                    }).ToListAsync();

                basketVM.BasketItemVMs.ForEach(b => basketVM.Total += b.SubTotal);
            }
            else
            {
                string json = _httpContext.Request.Cookies["Basket"];

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
            return basketVM;
        }

    }
}
