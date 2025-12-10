using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class LayoutService:ILayoutService
    {
        private readonly AppDBContext _context;

        public LayoutService(AppDBContext context)
        {
            _context = context;
        }

       
        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);


            return settings;
        }

    }
}
