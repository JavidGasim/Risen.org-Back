using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Risen.Business.Options;
using Risen.Entities.Entities;

namespace Risen.Web.Infrastructure
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class IdentitySeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services, IHostEnvironment env)
        {
            using var scope = services.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CustomIdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<CustomIdentityRole>>();

            // ===== CONFIG (buranı appsettings-ə də çıxara bilərsən) =====
            var adminEmail = "admin@gmail.com";
            var adminPassword = "Admin123!@#";
            var adminRole = "Admin";

            // ===== 1. ROLE CHECK =====
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                var roleResult = await roleManager.CreateAsync(new CustomIdentityRole
                {
                    Id = Guid.NewGuid(),
                    Name = adminRole
                });
                if (!roleResult.Succeeded)
                    throw new Exception("Admin role creation failed: " +
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            // ===== 2. USER CHECK =====
            var user = await userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                user = new CustomIdentityUser
                {
                    FirstName = "Admin",
                    LastName = " User",
                    UserName = adminEmail,
                    FullName = "Admin User",
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, adminPassword);

                if (!createResult.Succeeded)
                    throw new Exception("Admin user creation failed: " +
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
            else
            {
                // ===== 3. PASSWORD SYNC (kritik hissə) =====
                var hasPassword = await userManager.HasPasswordAsync(user);

                if (!hasPassword)
                {
                    var removeToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    var resetResult = await userManager.ResetPasswordAsync(user, removeToken, adminPassword);

                    if (!resetResult.Succeeded)
                        throw new Exception("Admin password reset failed: " +
                            string.Join(", ", resetResult.Errors.Select(e => e.Description)));
                }

                // Optional: force password sync (hər startup eyni password istəyirsə)
                // (istəsən açarsan)
            }

            // ===== 4. LOCKOUT FIX =====
            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);
            await userManager.SetLockoutEnabledAsync(user, false);

            // ===== 5. ROLE ASSIGN =====
            if (!await userManager.IsInRoleAsync(user, adminRole))
            {
                var roleResult = await userManager.AddToRoleAsync(user, adminRole);

                if (!roleResult.Succeeded)
                    throw new Exception("Assigning admin role failed: " +
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }

    }
}