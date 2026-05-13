using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Staff;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IStaffService
{
    Task<ApiResponse<StaffResponse>> CreateAsync(CreateStaffRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<StaffResponse>>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<StaffResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<StaffResponse>> UpdateAsync(Guid id, UpdateStaffRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid?> GetStaffIdByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
}
