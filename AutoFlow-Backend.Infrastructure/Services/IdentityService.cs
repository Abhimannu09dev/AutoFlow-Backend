using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Infrastructure.Entities;
using AutoFlow_Backend.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace AutoFlow_Backend.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<(bool Succeeded, string? UserId, string? Error)> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? phone,
        string? address,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Address = address,
            PhoneNumber = phone,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return (false, null, string.Join(" ", result.Errors.Select(e => e.Description)));

        return (true, user.Id.ToString(), null);
    }

    public async Task<(bool Succeeded, string? Error)> AssignRoleAsync(
        string userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return (false, "User not found.");

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded
            ? (true, null)
            : (false, string.Join(" ", result.Errors.Select(e => e.Description)));
    }

    public async Task<(bool Succeeded, string? Error)> UpdateUserAsync(
        string userId,
        string email,
        string firstName,
        string lastName,
        string? phone,
        string? address,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return (false, "Staff account is not available.");

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Address = address;
        user.PhoneNumber = phone;
        user.Email = email;
        user.UserName = email;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? (true, null)
            : (false, string.Join(" ", result.Errors.Select(e => e.Description)));
    }

    public async Task<(bool Succeeded, string? Error)> LockUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return (true, null);

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? (true, null)
            : (false, string.Join(" ", result.Errors.Select(e => e.Description)));
    }

    public async Task<bool> UserExistsByEmailAsync(
        string normalizedEmail,
        string? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
            return false;

        if (excludeUserId is not null && user.Id.ToString() == excludeUserId)
            return false;

        return true;
    }

    public async Task<bool> RoleExistsAsync(
        string role,
        CancellationToken cancellationToken = default) =>
        await _roleManager.RoleExistsAsync(role);

    public async Task DeleteUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
            await _userManager.DeleteAsync(user);
    }
}