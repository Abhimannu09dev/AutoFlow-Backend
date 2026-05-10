using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Sales;

namespace AutoFlow_Backend.Application.Interfaces;

public interface ICustomerSelfService
{
    Task<ApiResponse<List<SaleResponse>>> GetMyPurchasesAsync(
        Guid userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<List<AppointmentResponse>>> GetMyServicesAsync(
        Guid userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<CustomerResponseDto>> GetMyProfileAsync(
        Guid userId, CancellationToken cancellationToken = default);

    Task<ApiResponse<CustomerResponseDto>> UpdateMyProfileAsync(
        Guid userId, CustomerPatchDto request, CancellationToken cancellationToken = default);
}