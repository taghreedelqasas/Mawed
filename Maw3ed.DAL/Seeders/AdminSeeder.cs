using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public static class AdminSeeder
    {
      
            // Hardcoded admin credentials — change before production
            // or move to appsettings / user secrets.
            private const string AdminEmail = "admin@maw3ed.com";
            private const string AdminUserName = "superadmin";
            private const string AdminPassword = "Admin@123";
            private const string AdminSSN = "30102081200388";


        public static async Task SeedAsync(IServiceProvider services)
            {
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

                // Ensure Admin role exists.
                if (!await roleManager.RoleExistsAsync("Admin"))
                    await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });

                // If admin user already exists, skip.
                if (await userManager.FindByEmailAsync(AdminEmail) is not null)
                    return;

                var admin = new ApplicationUser
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    UserName = AdminUserName,
                    Email = AdminEmail,
                    SSN = AdminSSN,
                    EmailConfirmed = true,   // Admin doesn't need email confirmation.
                    IsActive = true
                };

                var result = await userManager.CreateAsync(admin, AdminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }

