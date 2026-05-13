using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PartRequests;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IPartRequestService
{
    Task<ApiResponse<PartRequestResponse>> CreateAsync(
        CreatePartRequestRequest request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResponse<PartRequestResponse>>> GetAllAsync(
        PagedRequest request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default);
}
