using Maw3ed.DAL;
using Microsoft.AspNetCore.Identity;

namespace Maw3ed.APIs
{
    public static class RoleSeederExtension
    {
        private static readonly string[] Roles = { "Admin", "Doctor", "Patient" };

        // Call this once at startup (after app.Build()) to make sure
        // the base roles exist before anyone tries to register/login.
        public static async Task SeedRolesAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }
    }
}

