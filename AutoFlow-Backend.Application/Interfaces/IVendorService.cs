using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Vendors;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IVendorService
{
    Task<ApiResponse<VendorResponse>> CreateAsync(CreateVendorRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<VendorResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<VendorResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<VendorResponse>> UpdateAsync(Guid id, UpdateVendorRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<VendorResponse>>> SearchAsync(string? query, CancellationToken cancellationToken = default);
}
