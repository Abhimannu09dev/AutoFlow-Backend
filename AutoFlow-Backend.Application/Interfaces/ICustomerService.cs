using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.DTOs.Vehicles;

namespace AutoFlow_Backend.Application.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<CustomerResponseDto>> CreateAsync(CustomerCreateDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<CustomerResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<SaleResponse>>> GetPurchasesAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<AppointmentResponse>>> GetServicesAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<CustomerResponseDto>>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerResponseDto>> UpdateAsync(Guid id, CustomerUpdateDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<VehicleResponseDto>> AddVehicleAsync(Guid customerId, VehicleCreateDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<VehicleResponseDto>>> GetVehiclesAsync(Guid customerId, CancellationToken cancellationToken = default);
}
