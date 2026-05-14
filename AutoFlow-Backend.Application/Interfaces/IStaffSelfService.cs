using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Staff;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IStaffSelfService
{
    Task<ApiResponse<StaffResponse>> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<StaffResponse>> UpdateMyProfileAsync(Guid userId, StaffPatchDto request, CancellationToken cancellationToken = default);
}
