using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PartRequests;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IPartRequestService
{
    Task<ApiResponse<PartRequestResponse>> CreateAsync(CreatePartRequestRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PartRequestResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
}
