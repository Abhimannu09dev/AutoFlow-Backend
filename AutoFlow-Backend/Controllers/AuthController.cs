using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Auth;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new customer account
    /// </summary>
    /// <param name="request">Registration details including FullName, Email, Password, optional Address and Phone</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration result with JWT token on success</returns>
    /// <response code="200">Registration successful with JWT token</response>
    /// <response code="400">Registration failed - email already exists or invalid data</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Authenticate user and get JWT token
    /// </summary>
    /// <param name="request">Login credentials (Email and Password)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login result with JWT token on success</returns>
    /// <response code="200">Login successful with JWT token</response>
    /// <response code="400">Login failed - invalid credentials</response>
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