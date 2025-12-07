using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDBContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequiredLength = 8;
    opt.Password.RequireNonAlphanumeric = false;

    opt.User.RequireUniqueEmail = true;
    
    opt.Lockout.MaxFailedAccessAttempts = 3;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);

}).AddEntityFrameworkStores<AppDBContext>().AddDefaultTokenProviders();


var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    "areas",
    "{area:exists}/{controller=home}/{action=index}/{id?}"

    );
app.MapControllerRoute(
    "default",
    "{controller=home}/{action=index}/{id?}"

    );


app.Run();
