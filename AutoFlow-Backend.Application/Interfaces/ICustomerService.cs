using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Customers;

namespace AutoFlow_Backend.Application.Interfaces;

public interface ICustomerService
{
    Task<APIResponse> CreateAsync(CustomerCreateDto request, CancellationToken cancellationToken = default);
    Task<APIResponse> GetAllAsync(CancellationToken cancellationToken = default);
    Task<APIResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<APIResponse> UpdateAsync(Guid id, CustomerUpdateDto request, CancellationToken cancellationToken = default);
    Task<APIResponse> AddVehicleAsync(Guid id, VehicleCreateDto request, CancellationToken cancellationToken = default);
    Task<APIResponse> GetVehiclesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<APIResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}