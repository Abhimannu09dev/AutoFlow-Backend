using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Parts;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IPartService
{
    Task<ApiResponse<PartResponse>> CreateAsync(CreatePartRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PartResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PartResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<PartResponse>> UpdateAsync(Guid id, UpdatePartRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PartResponse>>> SearchAsync(string? query, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PartResponse>>> GetLowStockAsync(CancellationToken cancellationToken = default);
}
