using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Auth;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default);
}