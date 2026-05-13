using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Vehicles;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IVehicleService
{
    Task<ApiResponse<VehicleResponseDto>> CreateAsync(
        VehicleCreateDto request,
        Guid? creatorUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResponse<VehicleResponseDto>>> GetAllAsync(
        PagedRequest request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<VehicleResponseDto>> GetByIdAsync(
        Guid id,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<VehicleResponseDto>>> GetMyVehiclesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<VehicleResponseDto>> UpdateAsync(
        Guid id,
        VehicleUpdateDto request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteAsync(
        Guid id,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default);

    Task<List<Guid>> GetUserIdsBySearchQueryAsync(string normalizedQuery, CancellationToken cancellationToken = default);
}