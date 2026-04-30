using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Customers;

namespace AutoFlow_Backend.Application.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<CustomerResponseDto>> CreateAsync(CustomerCreateDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<CustomerResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerResponseDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerResponseDto>> UpdateAsync(int id, CustomerUpdateDto request, CancellationToken cancellationToken = default);
}