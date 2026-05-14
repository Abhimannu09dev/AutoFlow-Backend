using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Models;
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
        string fullName,
        string? phone,
        string? address,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
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
        string fullName,
        string? phone,
        string? address,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return (false, "Staff account is not available.");

        user.FullName = fullName;
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

    public async Task<IdentityUserProfileReadModel?> GetUserProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new IdentityUserProfileReadModel(
            UserId: user.Id,
            Email: user.Email ?? string.Empty,
            FullName: user.FullName,
            Phone: user.PhoneNumber,
            Address: user.Address,
            Roles: roles.ToList());
    }

    public async Task<(bool Succeeded, string? Error)> UpdateUserProfileAsync(
        Guid userId,
        string fullName,
        string? phone,
        string? address,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return (false, "Admin profile not found.");

        user.FullName = fullName;
        user.PhoneNumber = phone;
        user.Address = address;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? (true, null)
            : (false, string.Join(" ", result.Errors.Select(e => e.Description)));
    }

    public async Task<(bool Succeeded, string? Error)> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return (false, "Admin profile not found.");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded
            ? (true, null)
            : (false, string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
