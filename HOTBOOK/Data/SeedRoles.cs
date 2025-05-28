using Microsoft.AspNetCore.Identity;
using HOTBOOK.Models;

namespace HOTBOOK.Data
{
    public static class SeedRoles
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("Staff"))
            {
                await roleManager.CreateAsync(new IdentityRole("Staff"));
            }

            if (!await roleManager.RoleExistsAsync("Guest"))
            {
                await roleManager.CreateAsync(new IdentityRole("Guest"));
            }
        }

        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            // Check if admin user exists
            var adminUser = await userManager.FindByEmailAsync("admin@hotbook.com");
            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@hotbook.com",
                    Email = "admin@hotbook.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

        public static async Task SeedGuestUserAsync(UserManager<ApplicationUser> userManager)
        {
            // Check if guest user exists
            var guestUser = await userManager.FindByEmailAsync("guest@hotbook.com");
            if (guestUser == null)
            {
                var guest = new ApplicationUser
                {
                    UserName = "guest@hotbook.com",
                    Email = "guest@hotbook.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(guest, "Guest123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(guest, "Guest");
                }
            }
        }
    }
} 