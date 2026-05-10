using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Auth;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Authentication")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IRegistrationService _registrationService;

    public AuthController(IAuthService authService, IRegistrationService registrationService)
    {
        _authService = authService;
        _registrationService = registrationService;
    }

    /// <summary>
    /// [Public] Register a new customer account (no authentication required)
    /// </summary>
    /// <param name="request">Registration details including FullName, Email, Password, optional Address and Phone</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration result with JWT token on success</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _registrationService.RegisterAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// [Public] Authenticate user and get JWT token (no authentication required)
    /// </summary>
    /// <param name="request">Login credentials (Email and Password)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login result with JWT token on success</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }
}