using Microsoft.AspNetCore.Identity;
using FoodOrderingSystem.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FoodOrderingSystem.ViewModels;

namespace FoodOrderingSystem.Services
{
    public class UserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
        }

        // Register a new user
        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            return result;
        }

        // Sign in an existing user
        public async Task<SignInResult> SignInAsync(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                _httpContextAccessor.HttpContext.Session.SetString("UserId", user.Id.ToString());
            }
            return result;
        }

        // Sign out the current user
        public async Task SignOutAsync()
        {
            _httpContextAccessor.HttpContext.Session.Remove("UserId");
            await _signInManager.SignOutAsync();
        }

        public async Task<bool> CheckPasswordAsync(IdentityUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        // Find a user by email
        public async Task<IdentityUser> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        // Create a new user
        public async Task<IdentityResult> CreateAsync(IdentityUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        // Add user to a role
        public async Task<IdentityResult> AddToRoleAsync(IdentityUser user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }
    }
}
