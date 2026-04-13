using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Risen.Business.Options;
using Risen.Entities.Entities;

namespace Risen.Web.Infrastructure
{
    public static class IdentitySeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services, IHostEnvironment env)
        {
            using var scope = services.CreateScope();

            var opt = scope.ServiceProvider
                .GetRequiredService<IOptions<AdminSeedOptions>>()
                .Value;

            // Dev və Test-də avtomatik icazə veririk.
            // Prod-da yalnız Enabled=true olsa işləyir.
            var allowByEnv = env.IsDevelopment() || env.IsEnvironment("Test");
            if (!allowByEnv && !opt.Enabled)
                return;

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<CustomIdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CustomIdentityUser>>();

            const string adminRole = "Admin";


            foreach (var role in new[] { "Admin", "Student", "University" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new CustomIdentityRole
                    {
                        Id = Guid.NewGuid(),
                        Name = role
                    });
                }
            }

            // 1) Admin rolunu yarat (yoxdursa)
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                var roleRes = await roleManager.CreateAsync(new CustomIdentityRole
                {
                    Id = Guid.NewGuid(),
                    Name = adminRole
                });

                if (!roleRes.Succeeded)
                    throw new InvalidOperationException("Cannot create Admin role: " +
                        string.Join(" | ", roleRes.Errors.Select(e => e.Description)));
            }

            // 2) Admin user-i tap (yoxdursa yarat)
            var email = (opt.Email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("AdminSeed:Email is required.");

            var adminUser = await userManager.FindByEmailAsync(email);

            if (adminUser is null)
            {
                // Yeni user yaradacağıqsa password mütləq lazımdır
                var password = opt.Password;
                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException(
                        "AdminSeed:Password is missing. " +
                        "Set it via User Secrets (Development) or Environment Variables (Test/CI).");
                }

                adminUser = new CustomIdentityUser
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,

                    FirstName = opt.FirstName.Trim(),
                    LastName = opt.LastName.Trim(),
                    FullName = $"{opt.FirstName} {opt.LastName}".Trim(),

                    CreatedAtUtc = DateTime.UtcNow
                };

                var createRes = await userManager.CreateAsync(adminUser, password);
                if (!createRes.Succeeded)
                    throw new InvalidOperationException("Cannot create Admin user: " +
                        string.Join(" | ", createRes.Errors.Select(e => e.Description)));
            }
            else
            {
                // Səndə DB-də FullName NOT NULL ola bilər — ehtiyat üçün düzəldirik
                if (string.IsNullOrWhiteSpace(adminUser.FullName))
                {
                    adminUser.FirstName = string.IsNullOrWhiteSpace(adminUser.FirstName) ? opt.FirstName : adminUser.FirstName;
                    adminUser.LastName = string.IsNullOrWhiteSpace(adminUser.LastName) ? opt.LastName : adminUser.LastName;
                    adminUser.FullName = $"{adminUser.FirstName} {adminUser.LastName}".Trim();

                    await userManager.UpdateAsync(adminUser);
                }
            }

            // 3) Admin rolunu ver (yoxdursa)
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                var addRoleRes = await userManager.AddToRoleAsync(adminUser, adminRole);

                if (!addRoleRes.Succeeded)
                    throw new InvalidOperationException("Cannot assign Admin role: " +
                        string.Join(" | ", addRoleRes.Errors.Select(e => e.Description)));
            }
        }
    }
}
