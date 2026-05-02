using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Staff;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Background.Infrastructure.Data;
using AutoFlow_Background.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Background.Infrastructure.Services;

public class StaffService : IStaffService
{
    private const string StaffRole = "Staff";
    private const int StaffCodeMaxLength = 30;
    private const int NameMaxLength = 100;
    private const int EmailMaxLength = 200;
    private const int PhoneMaxLength = 30;
    private const int AddressMaxLength = 300;
    private const int PositionMaxLength = 100;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly AppDbContext _dbContext;

    public StaffService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<StaffResponse>> CreateAsync(
        CreateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateForCreate(request);
        if (errors.Count > 0)
            return FailFromValidation<StaffResponse>(errors);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
            return Fail<StaffResponse>("Email is already registered.");

        var profileEmailExists = await _dbContext.Staffs
            .AsNoTracking()
            .AnyAsync(staff => staff.Email.ToLower() == normalizedEmail, cancellationToken);

        if (profileEmailExists)
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

        var createUserResult = await _userManager.CreateAsync(user, request.Password);
        if (!createUserResult.Succeeded)
            return Fail<StaffResponse>(string.Join(" ", createUserResult.Errors.Select(e => e.Description)));

        var assignRoleResult = await _userManager.AddToRoleAsync(user, StaffRole);
        if (!assignRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Fail<StaffResponse>(string.Join(" ", assignRoleResult.Errors.Select(e => e.Description)));
        }

        var staffCode = await ResolveStaffCodeAsync(request.StaffCode, cancellationToken);
        if (staffCode is null)
        {
            await _userManager.DeleteAsync(user);
            return Fail<StaffResponse>("Failed to generate a unique staff code.");
        }

        var staffProfile = new Staff
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = user.Id,
            StaffCode = staffCode,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = NormalizeOptional(request.Phone),
            Address = NormalizeOptional(request.Address),
            Position = NormalizeOptional(request.Position),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Staffs.Add(staffProfile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Staff created successfully.", Map(staffProfile));
    }

    public async Task<ApiResponse<List<StaffResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var staffProfiles = await _dbContext.Staffs
            .AsNoTracking()
            .OrderBy(staff => staff.FirstName)
            .ThenBy(staff => staff.LastName)
            .Select(staff => Map(staff))
            .ToListAsync(cancellationToken);

        return Success("Staff retrieved successfully.", staffProfiles);
    }

    public async Task<ApiResponse<StaffResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var staffProfile = await _dbContext.Staffs
            .AsNoTracking()
            .Where(staff => staff.Id == id)
            .Select(staff => Map(staff))
            .FirstOrDefaultAsync(cancellationToken);

        if (staffProfile is null)
            return Fail<StaffResponse>("Staff not found.");

        return Success("Staff retrieved successfully.", staffProfile);
    }

    public async Task<ApiResponse<StaffResponse>> UpdateAsync(
        Guid id,
        UpdateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateForUpdate(request);
        if (errors.Count > 0)
            return FailFromValidation<StaffResponse>(errors);

        var staffProfile = await _dbContext.Staffs
            .FirstOrDefaultAsync(staff => staff.Id == id, cancellationToken);

        if (staffProfile is null)
            return Fail<StaffResponse>("Staff not found.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExistsInProfiles = await _dbContext.Staffs
            .AsNoTracking()
            .AnyAsync(staff => staff.Id != id && staff.Email.ToLower() == normalizedEmail, cancellationToken);

        if (emailExistsInProfiles)
            return Fail<StaffResponse>("Email is already registered.");

        var user = await _userManager.FindByIdAsync(staffProfile.ApplicationUserId.ToString());
        if (user is null)
            return Fail<StaffResponse>("Staff account is not available.");

        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null && existingUser.Id != user.Id)
            return Fail<StaffResponse>("Email is already registered.");

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Address = NormalizeOptional(request.Address);
        user.PhoneNumber = NormalizeOptional(request.Phone);
        user.Email = normalizedEmail;
        user.UserName = normalizedEmail;
        user.UpdatedAt = DateTime.UtcNow;

        var updateUserResult = await _userManager.UpdateAsync(user);
        if (!updateUserResult.Succeeded)
            return Fail<StaffResponse>(string.Join(" ", updateUserResult.Errors.Select(e => e.Description)));

        staffProfile.FirstName = request.FirstName.Trim();
        staffProfile.LastName = request.LastName.Trim();
        staffProfile.Email = normalizedEmail;
        staffProfile.PhoneNumber = NormalizeOptional(request.Phone);
        staffProfile.Address = NormalizeOptional(request.Address);
        staffProfile.Position = NormalizeOptional(request.Position);
        staffProfile.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Staff updated successfully.", Map(staffProfile));
    }

    public async Task<ApiResponse<bool>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var staffProfile = await _dbContext.Staffs
            .FirstOrDefaultAsync(staff => staff.Id == id, cancellationToken);

        if (staffProfile is null)
            return Fail<bool>("Staff not found.");

        if (!staffProfile.IsActive)
            return Fail<bool>("Staff is already deactivated.");

        var user = await _userManager.FindByIdAsync(staffProfile.ApplicationUserId.ToString());
        if (user is not null)
        {
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            user.UpdatedAt = DateTime.UtcNow;

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
                return Fail<bool>(string.Join(" ", updateUserResult.Errors.Select(e => e.Description)));
        }

        staffProfile.IsActive = false;
        staffProfile.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Staff deactivated successfully.", true);
    }

    private async Task<string?> ResolveStaffCodeAsync(string? requestedCode, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requestedCode))
        {
            var normalizedCode = requestedCode.Trim().ToUpperInvariant();
            var exists = await _dbContext.Staffs
                .AsNoTracking()
                .AnyAsync(staff => staff.StaffCode == normalizedCode, cancellationToken);

            return exists ? null : normalizedCode;
        }

        for (var attempt = 0; attempt < 10; attempt++)
        {
            var generatedCode = $"STF-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
            var exists = await _dbContext.Staffs
                .AsNoTracking()
                .AnyAsync(staff => staff.StaffCode == generatedCode, cancellationToken);

            if (!exists)
                return generatedCode;
        }

        return null;
    }

    private static StaffResponse Map(Staff staff)
    {
        return new StaffResponse
        {
            Id = staff.Id,
            ApplicationUserId = staff.ApplicationUserId,
            StaffCode = staff.StaffCode,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            Email = staff.Email,
            Phone = staff.PhoneNumber,
            Address = staff.Address,
            Position = staff.Position,
            IsActive = staff.IsActive,
            CreatedAt = staff.CreatedAt,
            UpdatedAt = staff.UpdatedAt
        };
    }

    private static List<string> ValidateForCreate(CreateStaffRequest request)
    {
        var errors = ValidateCommon(
            request.StaffCode,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Address,
            request.Position);

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required.");

        return errors;
    }

    private static List<string> ValidateForUpdate(UpdateStaffRequest request)
    {
        return ValidateCommon(
            null,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Address,
            request.Position);
    }

    private static List<string> ValidateCommon(
        string? staffCode,
        string? firstName,
        string? lastName,
        string? email,
        string? phone,
        string? address,
        string? position)
    {
        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(staffCode) && staffCode.Trim().Length > StaffCodeMaxLength)
            errors.Add($"Staff code must be at most {StaffCodeMaxLength} characters.");

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

        if (!string.IsNullOrWhiteSpace(position) && position.Trim().Length > PositionMaxLength)
            errors.Add($"Position must be at most {PositionMaxLength} characters.");

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
