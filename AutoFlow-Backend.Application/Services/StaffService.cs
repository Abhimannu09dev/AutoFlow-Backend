using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Staff;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Application.Mappers;
using AutoFlow_Backend.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AutoFlow_Backend.Application.Services;

public class StaffService : IStaffService
{
    private const string StaffRole = "Staff";

    private readonly IIdentityService _identityService;
    private readonly IStaffRepository _staffRepository;
    private readonly IValidator<CreateStaffRequest> _createValidator;
    private readonly IValidator<UpdateStaffRequest> _updateValidator;
    private readonly ILogger<StaffService> _logger;

    public StaffService(
        IIdentityService identityService,
        IStaffRepository staffRepository,
        IValidator<CreateStaffRequest> createValidator,
        IValidator<UpdateStaffRequest> updateValidator,
        ILogger<StaffService> logger)
    {
        _identityService = identityService;
        _staffRepository = staffRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<ApiResponse<StaffResponse>> CreateAsync(
        CreateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponseFactory.FailFromValidation<StaffResponse>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var userEmailExists = await _identityService.UserExistsByEmailAsync(normalizedEmail, null, cancellationToken);
        if (userEmailExists)
            return ApiResponseFactory.FailConflict<StaffResponse>("Email is already registered.");

        var profileEmailExists = await _staffRepository.EmailExistsAsync(normalizedEmail, null, cancellationToken);
        if (profileEmailExists)
            return ApiResponseFactory.FailConflict<StaffResponse>("Email is already registered.");

        var role = string.IsNullOrWhiteSpace(request.Role) ? StaffRole : request.Role.Trim();
        if (role != "Staff" && role != "Admin")
            return ApiResponseFactory.Fail<StaffResponse>("Role must be either 'Staff' or 'Admin'.");

        var roleExists = await _identityService.RoleExistsAsync(role, cancellationToken);
        if (!roleExists)
            return ApiResponseFactory.Fail<StaffResponse>($"Role '{role}' is not configured.");

        var (createSucceeded, userId, createError) = await _identityService.CreateUserAsync(
            email: normalizedEmail,
            password: request.Password,
            fullName: request.FullName.Trim(),
            phone: StringNormalizer.NormalizeOptional(request.Phone),
            address: StringNormalizer.NormalizeOptional(request.Address),
            cancellationToken: cancellationToken);

        if (!createSucceeded || userId is null)
            return ApiResponseFactory.Fail<StaffResponse>(createError ?? "Failed to create user account.");

        var (assignSucceeded, assignError) = await _identityService.AssignRoleAsync(userId, role, cancellationToken);
        if (!assignSucceeded)
        {
            await _identityService.DeleteUserAsync(userId, cancellationToken);
            return ApiResponseFactory.Fail<StaffResponse>(assignError ?? "Failed to assign role.");
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
            PhoneNumber = StringNormalizer.NormalizeOptional(request.Phone),
            Address = StringNormalizer.NormalizeOptional(request.Address),
            Position = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _staffRepository.AddAsync(staffProfile, cancellationToken);
        await _staffRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Staff created successfully.", staffProfile.ToResponse());
    }

    public async Task<ApiResponse<List<StaffResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var staffProfiles = await _staffRepository.GetAllAsync(cancellationToken);
        return ApiResponseFactory.Ok("Staff retrieved successfully.", staffProfiles.Select(s => s.ToResponse()).ToList());
    }

    public async Task<ApiResponse<StaffResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var staffProfile = await _staffRepository.GetByIdAsync(id, cancellationToken);
        if (staffProfile is null)
            return ApiResponseFactory.FailNotFound<StaffResponse>("Staff not found.");

        return ApiResponseFactory.Ok("Staff retrieved successfully.", staffProfile.ToResponse());
    }

    public async Task<ApiResponse<StaffResponse>> UpdateAsync(
        Guid id,
        UpdateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return ApiResponseFactory.FailFromValidation<StaffResponse>(
                validationResult.Errors.Select(e => e.ErrorMessage).ToList());

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
            phone: StringNormalizer.NormalizeOptional(request.Phone),
            address: StringNormalizer.NormalizeOptional(request.Address),
            cancellationToken: cancellationToken);

        if (!updateSucceeded)
            return ApiResponseFactory.Fail<StaffResponse>(updateError ?? "Failed to update user account.");

        staffProfile.FullName = request.FullName.Trim();
        staffProfile.Email = normalizedEmail;
        staffProfile.PhoneNumber = StringNormalizer.NormalizeOptional(request.Phone);
        staffProfile.Address = StringNormalizer.NormalizeOptional(request.Address);
        staffProfile.Position = StringNormalizer.NormalizeOptional(request.Position);
        staffProfile.UpdatedAt = DateTime.UtcNow;

        _staffRepository.Update(staffProfile);
        await _staffRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Staff updated successfully.", staffProfile.ToResponse());
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

    public async Task<Guid?> GetStaffIdByApplicationUserIdAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken = default)
    {
        var staff = await _staffRepository.GetActiveByApplicationUserIdAsync(applicationUserId, cancellationToken);
        return staff?.Id;
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
}