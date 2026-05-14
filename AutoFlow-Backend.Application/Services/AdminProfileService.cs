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

        var response = await BuildProfileResponseAsync(requestingUserId.Value, cancellationToken);
        if (response is null)
            return ApiResponseFactory.FailNotFound<AdminProfileResponse>("Admin profile not found.");

        return ApiResponseFactory.Ok("Admin profile retrieved successfully.", response);
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

        var response = await BuildProfileResponseAsync(requestingUserId.Value, cancellationToken);
        if (response is null)
            return ApiResponseFactory.FailNotFound<AdminProfileResponse>("Admin profile not found.");

        return ApiResponseFactory.Ok("Admin profile updated successfully.", response);
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

    private async Task<AdminProfileResponse?> BuildProfileResponseAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var profile = await _identityService.GetUserProfileAsync(userId, cancellationToken);
        if (profile is null)
            return null;

        var staffProfile = await _staffRepository.GetByApplicationUserIdAsync(userId, cancellationToken);

        var role = profile.Roles.FirstOrDefault(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                   ?? profile.Roles.FirstOrDefault()
                   ?? "Admin";

        return new AdminProfileResponse
        {
            Id = profile.UserId,
            FullName = profile.FullName,
            Email = profile.Email,
            Phone = staffProfile?.PhoneNumber ?? profile.Phone,
            Address = staffProfile?.Address ?? profile.Address,
            Role = role
        };
    }
}
