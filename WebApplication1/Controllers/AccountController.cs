using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Models;
using WebApplication1.Utilities.Extensions;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebHostEnvironment _env;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM userVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            int age = DateTime.Now.Year - userVM.Birthday.Year;
            if (age <= 18)
            {
                ModelState.AddModelError(string.Empty, "Under 18 not allowed");
                return View();
            }
            string image = "/assets/images/website-images/User-avatar.svg.png";


            string file = await userVM.ProfileImage
                .CreateFileAsync(_env.WebRootPath, "uploads", "profiles");
            string imagePath = $"/uploads/profiles/{file}";
            if (image != null)
            {
                image = imagePath;
            }

            AppUser user = new AppUser()
            {
                UserName = userVM.UserName,
                Email = userVM.Email,
                Name = userVM.Name,
                Surname = userVM.Surname,
                PhoneNumber = userVM.PhoneNumber,
                Birthday = userVM.Birthday,
                Gender = userVM.Gender,
                ProfileImage = image


            };
            IdentityResult result = await _userManager.CreateAsync(user, userVM.Password);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Index", "Home");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM userVM, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            AppUser? user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == userVM.UserNameorEmail || u.Email == userVM.UserNameorEmail);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Username, Email or Password is incorrect.");
                return View();
            }
            var result = await _signInManager.PasswordSignInAsync(user, userVM.Password, userVM.IsPersistant, true);
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account is blocked please try later.");
                return View();

            }
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Username, Email or Password is incorrect.");
                return View();
            }
            if (returnUrl != null)
            {

                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
