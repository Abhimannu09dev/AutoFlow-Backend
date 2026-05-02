using AutoFlow_Background.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoFlow_Background.Infrastructure.Identity;

public static class IdentitySeeder
{
    private static readonly string[] Roles = { "Admin", "Staff", "Customer" };

    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        var seedSettings = configuration.GetSection("SeedAdmin").Get<SeedAdminSettings>();
        if (seedSettings is null || !seedSettings.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(seedSettings.Email))
        {
            throw new InvalidOperationException("SeedAdmin.Email is required when SeedAdmin.Enabled is true.");
        }

        if (string.IsNullOrWhiteSpace(seedSettings.Password))
        {
            throw new InvalidOperationException("SeedAdmin.Password is required when SeedAdmin.Enabled is true.");
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = seedSettings.Email.Trim();
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = string.IsNullOrWhiteSpace(seedSettings.FirstName) ? "System" : seedSettings.FirstName.Trim(),
                LastName = string.IsNullOrWhiteSpace(seedSettings.LastName) ? "Admin" : seedSettings.LastName.Trim(),
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(adminUser, seedSettings.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(" ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to seed admin user from SeedAdmin settings. {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!addToRoleResult.Succeeded)
            {
                var errors = string.Join(" ", addToRoleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Failed to assign Admin role to seeded user. {errors}");
            }
        }
    }

    private sealed class SeedAdminSettings
    {
        public bool Enabled { get; init; }
        public string? Email { get; init; }
        public string? Password { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
    }
}
