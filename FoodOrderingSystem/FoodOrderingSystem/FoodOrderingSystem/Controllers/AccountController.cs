using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingSystem.Data;
using System.Threading.Tasks;
using FoodOrderingSystem.Models;
using FoodOrderingSystem.ViewModels;

namespace FoodOrderingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                // Check if password is correct
                var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);
                if (passwordCheck)
                {
                    // Sign in using the username, not the user object
                    var result = await _signInManager.PasswordSignInAsync(user.Email, model.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Checkout");
                    }
                }
                // Password is incorrect
                TempData["Error"] = "Wrong credentials. Please try again";
                return View(model);
            }
            // User not found
            TempData["Error"] = "Wrong credentials. Please try again";
            return View(model);
        }


        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            var response = new RegisterViewModel();
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                TempData["Error"] = "This email address is already in use";
                return View(model);
            }

            var newUser = new IdentityUser()
            {
                Email = model.Email,
                UserName = model.Email
            };

            var newUserResponse = await _userManager.CreateAsync(newUser, model.Password);

            if (newUserResponse.Succeeded)
            {
                // Sign in the user after successful registration
                var signInResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);
                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index", "Checkout");
                }
                else
                {
                    // Handle sign-in failure if necessary
                    TempData["Error"] = "Registration successful, but unable to sign in.";
                    return RedirectToAction("Login", "Account");
                }
            }

            foreach (var error in newUserResponse.Errors)
            {
                TempData["Error"] = error.Description;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
