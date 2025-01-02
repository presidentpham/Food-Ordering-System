using eCommerceApp.Models;
using Microsoft.AspNetCore.Identity;

namespace eCommerceApp.Services
{
    public class RegisterService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public RegisterService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            return await _userManager.CreateAsync(user, model.Password);
        }
    }
}
