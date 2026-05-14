using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Admin;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;

namespace AutoFlow_Backend.Application.Services;

public class AdminProfileService : IAdminProfileService
{
    private readonly IIdentityService _identityService;
    private readonly IStaffRepository _staffRepository;

    public AdminProfileService(
        IIdentityService identityService,
        IStaffRepository staffRepository)
    {
        _identityService = identityService;
        _staffRepository = staffRepository;
    }

    public async Task<ApiResponse<AdminProfileResponse>> GetProfileAsync(
        Guid? requestingUserId,
        CancellationToken cancellationToken = default)
    {
        if (!requestingUserId.HasValue)
            return ApiResponseFactory.Fail<AdminProfileResponse>("Unauthorized request.", ErrorType.Unauthorized);

        var profile = await _identityService.GetUserProfileAsync(requestingUserId.Value, cancellationToken);
        if (profile is null)
            return ApiResponseFactory.FailNotFound<AdminProfileResponse>("Admin profile not found.");

        var role = profile.Roles.FirstOrDefault(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                   ?? profile.Roles.FirstOrDefault()
                   ?? "Admin";

        return ApiResponseFactory.Ok("Admin profile retrieved successfully.", new AdminProfileResponse
        {
            Id = profile.UserId,
            FullName = profile.FullName,
            Email = profile.Email,
            Phone = profile.Phone,
            Address = profile.Address,
            Role = role
        });
    }

    public async Task<ApiResponse<AdminProfileResponse>> UpdateProfileAsync(
        Guid? requestingUserId,
        UpdateAdminProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!requestingUserId.HasValue)
            return ApiResponseFactory.Fail<AdminProfileResponse>("Unauthorized request.", ErrorType.Unauthorized);

        if (string.IsNullOrWhiteSpace(request.FullName))
            return ApiResponseFactory.Fail<AdminProfileResponse>("FullName is required.");

        var normalizedFullName = request.FullName.Trim();
        var normalizedPhone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        var normalizedAddress = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

        var updateResult = await _identityService.UpdateUserProfileAsync(
            requestingUserId.Value,
            normalizedFullName,
            normalizedPhone,
            normalizedAddress,
            cancellationToken);

        if (!updateResult.Succeeded)
            return ApiResponseFactory.Fail<AdminProfileResponse>(updateResult.Error ?? "Failed to update admin profile.");

        var staffProfile = await _staffRepository.GetByApplicationUserIdAsync(requestingUserId.Value, cancellationToken);
        if (staffProfile is not null)
        {
            staffProfile.FullName = normalizedFullName;
            staffProfile.PhoneNumber = normalizedPhone;
            staffProfile.Address = normalizedAddress;
            staffProfile.UpdatedAt = DateTime.UtcNow;
            _staffRepository.Update(staffProfile);
            await _staffRepository.SaveChangesAsync(cancellationToken);
        }

        var refreshed = await _identityService.GetUserProfileAsync(requestingUserId.Value, cancellationToken);
        if (refreshed is null)
            return ApiResponseFactory.FailNotFound<AdminProfileResponse>("Admin profile not found.");

        var role = refreshed.Roles.FirstOrDefault(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                   ?? refreshed.Roles.FirstOrDefault()
                   ?? "Admin";

        return ApiResponseFactory.Ok("Admin profile updated successfully.", new AdminProfileResponse
        {
            Id = refreshed.UserId,
            FullName = refreshed.FullName,
            Email = refreshed.Email,
            Phone = refreshed.Phone,
            Address = refreshed.Address,
            Role = role
        });
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(
        Guid? requestingUserId,
        ChangeAdminPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!requestingUserId.HasValue)
            return ApiResponseFactory.Fail<bool>("Unauthorized request.", ErrorType.Unauthorized);

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return ApiResponseFactory.Fail<bool>("CurrentPassword is required.");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return ApiResponseFactory.Fail<bool>("NewPassword is required.");

        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
            return ApiResponseFactory.Fail<bool>("ConfirmPassword must match NewPassword.");

        var passwordResult = await _identityService.ChangePasswordAsync(
            requestingUserId.Value,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);

        if (!passwordResult.Succeeded)
            return ApiResponseFactory.Fail<bool>(passwordResult.Error ?? "Failed to change password.");

        return ApiResponseFactory.Ok("Admin password changed successfully.", true);
    }
}
