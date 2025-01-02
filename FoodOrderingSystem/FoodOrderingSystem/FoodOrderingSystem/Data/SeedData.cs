using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace FoodOrderingSystem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Define the roles to be created
            string[] roleNames = { "Admin", "User" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the role
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create a default user
            var user = await userManager.FindByEmailAsync("admin@example.com");
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com"
                };
                await userManager.CreateAsync(user, "Test@123");
            }

            // Assign the "Admin" role to the user
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
