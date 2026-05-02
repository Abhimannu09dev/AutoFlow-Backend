using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Staff;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Background.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace AutoFlow_Background.Infrastructure.Services;

public class StaffService : IStaffService
{
    private const string StaffRole = "Staff";
    private const int NameMaxLength = 100;
    private const int EmailMaxLength = 200;
    private const int PhoneMaxLength = 30;
    private const int AddressMaxLength = 300;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public StaffService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ApiResponse<StaffResponse>> CreateAsync(
        CreateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateForCreate(request);
        if (errors.Count > 0)
            return FailFromValidation<StaffResponse>(errors);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existing = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existing is not null)
            return Fail<StaffResponse>("Email is already registered.");

        if (!await _roleManager.RoleExistsAsync(StaffRole))
            return Fail<StaffResponse>("Staff role is not configured.");

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Address = NormalizeOptional(request.Address),
            PhoneNumber = NormalizeOptional(request.Phone),
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return Fail<StaffResponse>(string.Join(" ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await _userManager.AddToRoleAsync(user, StaffRole);
        if (!roleResult.Succeeded)
            return Fail<StaffResponse>(string.Join(" ", roleResult.Errors.Select(e => e.Description)));

        return Success("Staff created successfully.", Map(user));
    }

    public async Task<ApiResponse<List<StaffResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.GetUsersInRoleAsync(StaffRole);

        var staff = users
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(Map)
            .ToList();

        return Success("Staff retrieved successfully.", staff);
    }

    public async Task<ApiResponse<StaffResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return Fail<StaffResponse>("Staff not found.");

        if (!await _userManager.IsInRoleAsync(user, StaffRole))
            return Fail<StaffResponse>("Staff not found.");

        return Success("Staff retrieved successfully.", Map(user));
    }

    public async Task<ApiResponse<StaffResponse>> UpdateAsync(
        Guid id,
        UpdateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateForUpdate(request);
        if (errors.Count > 0)
            return FailFromValidation<StaffResponse>(errors);

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return Fail<StaffResponse>("Staff not found.");

        if (!await _userManager.IsInRoleAsync(user, StaffRole))
            return Fail<StaffResponse>("Staff not found.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existing = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existing is not null && existing.Id != user.Id)
            return Fail<StaffResponse>("Email is already registered.");

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Address = NormalizeOptional(request.Address);
        user.PhoneNumber = NormalizeOptional(request.Phone);
        user.Email = normalizedEmail;
        user.UserName = normalizedEmail;
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Fail<StaffResponse>(string.Join(" ", updateResult.Errors.Select(e => e.Description)));

        return Success("Staff updated successfully.", Map(user));
    }

    public async Task<ApiResponse<bool>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return Fail<bool>("Staff not found.");

        if (!await _userManager.IsInRoleAsync(user, StaffRole))
            return Fail<bool>("Staff not found.");

        if (!IsActive(user))
            return Fail<bool>("Staff is already deactivated.");

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Fail<bool>(string.Join(" ", updateResult.Errors.Select(e => e.Description)));

        return Success("Staff deactivated successfully.", true);
    }

    private static StaffResponse Map(ApplicationUser user)
    {
        return new StaffResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber,
            Address = user.Address,
            IsActive = IsActive(user),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    private static bool IsActive(ApplicationUser user)
    {
        return !user.LockoutEnabled || user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow;
    }

    private static List<string> ValidateForCreate(CreateStaffRequest request)
    {
        var errors = ValidateCommon(request.FirstName, request.LastName, request.Email, request.Phone, request.Address);

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required.");

        return errors;
    }

    private static List<string> ValidateForUpdate(UpdateStaffRequest request)
    {
        return ValidateCommon(request.FirstName, request.LastName, request.Email, request.Phone, request.Address);
    }

    private static List<string> ValidateCommon(
        string? firstName,
        string? lastName,
        string? email,
        string? phone,
        string? address)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(firstName))
            errors.Add("First name is required.");
        else if (firstName.Trim().Length > NameMaxLength)
            errors.Add($"First name must be at most {NameMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(lastName))
            errors.Add("Last name is required.");
        else if (lastName.Trim().Length > NameMaxLength)
            errors.Add($"Last name must be at most {NameMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(email))
            errors.Add("Email is required.");
        else if (email.Trim().Length > EmailMaxLength)
            errors.Add($"Email must be at most {EmailMaxLength} characters.");
        else if (!IsValidEmail(email.Trim()))
            errors.Add("Email must be valid.");

        if (!string.IsNullOrWhiteSpace(phone) && phone.Trim().Length > PhoneMaxLength)
            errors.Add($"Phone must be at most {PhoneMaxLength} characters.");

        if (!string.IsNullOrWhiteSpace(address) && address.Trim().Length > AddressMaxLength)
            errors.Add($"Address must be at most {AddressMaxLength} characters.");

        return errors;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var _ = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static ApiResponse<T> Success<T>(string message, T data)
    {
        return new ApiResponse<T>
        {
            Status = true,
            Message = message,
            Data = data
        };
    }

    private static ApiResponse<T> Fail<T>(string message)
    {
        return new ApiResponse<T>
        {
            Status = false,
            Message = message,
            Data = default
        };
    }

    private static ApiResponse<T> FailFromValidation<T>(List<string> errors)
    {
        var message = errors.Count > 0 ? string.Join(" ", errors) : "Validation failed.";
        return Fail<T>(message);
    }
}
