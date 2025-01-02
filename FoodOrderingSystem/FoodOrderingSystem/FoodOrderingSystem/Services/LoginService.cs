using Microsoft.AspNetCore.Identity;

namespace eCommerceApp.Services
{
    public class LoginService
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public LoginService(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<bool> LoginUserAsync(string email, string password, bool rememberMe)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
            return result.Succeeded;
        }
    }
}
