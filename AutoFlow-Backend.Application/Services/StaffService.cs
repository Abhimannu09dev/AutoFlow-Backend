using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Staff;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services;

public class StaffService : IStaffService
{
    private const string StaffRole = "Staff";
    private const int StaffCodeMaxLength = 30;
    private const int FullNameMaxLength = 200;
    private const int EmailMaxLength = 200;
    private const int PhoneMaxLength = 30;
    private const int AddressMaxLength = 300;
    private const int PositionMaxLength = 100;

    private readonly IIdentityService _identityService;
    private readonly IStaffRepository _staffRepository;

    public StaffService(
        IIdentityService identityService,
        IStaffRepository staffRepository)
    {
        _identityService = identityService;
        _staffRepository = staffRepository;
    }

    public async Task<ApiResponse<StaffResponse>> CreateAsync(
        CreateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateForCreate(request);
        if (errors.Count > 0)
            return ApiResponseFactory.FailFromValidation<StaffResponse>(errors);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var userEmailExists = await _identityService.UserExistsByEmailAsync(normalizedEmail, null, cancellationToken);
        if (userEmailExists)
            return ApiResponseFactory.FailConflict<StaffResponse>("Email is already registered.");

        var profileEmailExists = await _staffRepository.EmailExistsAsync(normalizedEmail, null, cancellationToken);
        if (profileEmailExists)
            return ApiResponseFactory.FailConflict<StaffResponse>("Email is already registered.");

        var roleExists = await _identityService.RoleExistsAsync(StaffRole, cancellationToken);
        if (!roleExists)
            return ApiResponseFactory.Fail<StaffResponse>("Staff role is not configured.");

        var (createSucceeded, userId, createError) = await _identityService.CreateUserAsync(
            email: normalizedEmail,
            password: request.Password,
            fullName: request.FullName.Trim(),
            phone: NormalizeOptional(request.Phone),
            address: NormalizeOptional(request.Address),
            cancellationToken: cancellationToken);

        if (!createSucceeded || userId is null)
            return ApiResponseFactory.Fail<StaffResponse>(createError ?? "Failed to create user account.");

        var (assignSucceeded, assignError) = await _identityService.AssignRoleAsync(userId, StaffRole, cancellationToken);
        if (!assignSucceeded)
        {
            await _identityService.DeleteUserAsync(userId, cancellationToken);
            return ApiResponseFactory.Fail<StaffResponse>(assignError ?? "Failed to assign staff role.");
        }

        var staffCode = await ResolveStaffCodeAsync(request.StaffCode, cancellationToken);
        if (staffCode is null)
        {
            await _identityService.DeleteUserAsync(userId, cancellationToken);
            return ApiResponseFactory.Fail<StaffResponse>("Failed to generate a unique staff code.");
        }

        var staffProfile = new Staff
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = Guid.Parse(userId),
            StaffCode = staffCode,
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = NormalizeOptional(request.Phone),
            Address = NormalizeOptional(request.Address),
            Position = NormalizeOptional(request.Position),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _staffRepository.AddAsync(staffProfile, cancellationToken);
        await _staffRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Staff created successfully.", Map(staffProfile));
    }

    public async Task<ApiResponse<List<StaffResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var staffProfiles = await _staffRepository.GetAllAsync(cancellationToken);
        return ApiResponseFactory.Ok("Staff retrieved successfully.", staffProfiles.Select(Map).ToList());
    }

    public async Task<ApiResponse<StaffResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var staffProfile = await _staffRepository.GetByIdAsync(id, cancellationToken);
        if (staffProfile is null)
            return ApiResponseFactory.FailNotFound<StaffResponse>("Staff not found.");

        return ApiResponseFactory.Ok("Staff retrieved successfully.", Map(staffProfile));
    }

    public async Task<ApiResponse<StaffResponse>> UpdateAsync(
        Guid id,
        UpdateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ValidateForUpdate(request);
        if (errors.Count > 0)
            return ApiResponseFactory.FailFromValidation<StaffResponse>(errors);

        var staffProfile = await _staffRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (staffProfile is null)
            return ApiResponseFactory.FailNotFound<StaffResponse>("Staff not found.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var profileEmailTaken = await _staffRepository.EmailExistsAsync(normalizedEmail, id, cancellationToken);
        if (profileEmailTaken)
            return ApiResponseFactory.FailConflict<StaffResponse>("Email is already registered.");

        var userEmailTaken = await _identityService.UserExistsByEmailAsync(
            normalizedEmail, staffProfile.ApplicationUserId.ToString(), cancellationToken);
        if (userEmailTaken)
            return ApiResponseFactory.FailConflict<StaffResponse>("Email is already registered.");

        var (updateSucceeded, updateError) = await _identityService.UpdateUserAsync(
            userId: staffProfile.ApplicationUserId.ToString(),
            email: normalizedEmail,
            fullName: request.FullName.Trim(),
            phone: NormalizeOptional(request.Phone),
            address: NormalizeOptional(request.Address),
            cancellationToken: cancellationToken);

        if (!updateSucceeded)
            return ApiResponseFactory.Fail<StaffResponse>(updateError ?? "Failed to update user account.");

        staffProfile.FullName = request.FullName.Trim();
        staffProfile.Email = normalizedEmail;
        staffProfile.PhoneNumber = NormalizeOptional(request.Phone);
        staffProfile.Address = NormalizeOptional(request.Address);
        staffProfile.Position = NormalizeOptional(request.Position);
        staffProfile.UpdatedAt = DateTime.UtcNow;

        _staffRepository.Update(staffProfile);
        await _staffRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Staff updated successfully.", Map(staffProfile));
    }

    public async Task<ApiResponse<bool>> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var staffProfile = await _staffRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (staffProfile is null)
            return ApiResponseFactory.FailNotFound<bool>("Staff not found.");

        if (!staffProfile.IsActive)
            return ApiResponseFactory.Fail<bool>("Staff is already deactivated.");

        var (lockSucceeded, lockError) = await _identityService.LockUserAsync(
            staffProfile.ApplicationUserId.ToString(), cancellationToken);

        if (!lockSucceeded)
            return ApiResponseFactory.Fail<bool>(lockError ?? "Failed to lock user account.");

        staffProfile.IsActive = false;
        staffProfile.UpdatedAt = DateTime.UtcNow;

        _staffRepository.Update(staffProfile);
        await _staffRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Staff deactivated successfully.", true);
    }

    private async Task<string?> ResolveStaffCodeAsync(
        string? requestedCode,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requestedCode))
        {
            var normalizedCode = requestedCode.Trim().ToUpperInvariant();
            var exists = await _staffRepository.StaffCodeExistsAsync(normalizedCode, cancellationToken);
            return exists ? null : normalizedCode;
        }

        for (var attempt = 0; attempt < 10; attempt++)
        {
            var generatedCode = $"STF-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
            var exists = await _staffRepository.StaffCodeExistsAsync(generatedCode, cancellationToken);
            if (!exists)
                return generatedCode;
        }

        return null;
    }

    private static StaffResponse Map(Staff staff) => new()
    {
        Id = staff.Id,
        ApplicationUserId = staff.ApplicationUserId,
        StaffCode = staff.StaffCode,
        FullName = staff.FullName,
        Email = staff.Email,
        Phone = staff.PhoneNumber,
        Address = staff.Address,
        Position = staff.Position,
        IsActive = staff.IsActive,
        CreatedAt = staff.CreatedAt,
        UpdatedAt = staff.UpdatedAt
    };

    private static List<string> ValidateForCreate(CreateStaffRequest request)
    {
        var errors = ValidateCommon(
            request.StaffCode, request.FullName,
            request.Email, request.Phone, request.Address, request.Position);

        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required.");

        return errors;
    }

    private static List<string> ValidateForUpdate(UpdateStaffRequest request) =>
        ValidateCommon(null, request.FullName,
            request.Email, request.Phone, request.Address, request.Position);

    private static List<string> ValidateCommon(
        string? staffCode, string? fullName,
        string? email, string? phone, string? address, string? position)
    {
        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(staffCode) && staffCode.Trim().Length > StaffCodeMaxLength)
            errors.Add($"Staff code must be at most {StaffCodeMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(fullName))
            errors.Add("Full name is required.");
        else if (fullName.Trim().Length > FullNameMaxLength)
            errors.Add($"Full name must be at most {FullNameMaxLength} characters.");

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

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
}