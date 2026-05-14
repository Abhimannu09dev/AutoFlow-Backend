using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Admin;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IAdminProfileService
{
    Task<ApiResponse<AdminProfileResponse>> GetProfileAsync(
        Guid? requestingUserId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<AdminProfileResponse>> UpdateProfileAsync(
        Guid? requestingUserId,
        UpdateAdminProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> ChangePasswordAsync(
        Guid? requestingUserId,
        ChangeAdminPasswordRequest request,
        CancellationToken cancellationToken = default);
}
