using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Auth;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IRegistrationService
{
    Task<ApiResponse<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);
}