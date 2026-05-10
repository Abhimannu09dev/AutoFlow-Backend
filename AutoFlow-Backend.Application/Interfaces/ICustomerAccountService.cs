using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.Models;

namespace AutoFlow_Backend.Application.Interfaces;

public interface ICustomerAccountService
{
    Task<ApiResponse<CustomerAccountResult>> ProvisionAsync(
        string fullName,
        string normalizedEmail,
        string? phone,
        string? address,
        CancellationToken cancellationToken = default);
}