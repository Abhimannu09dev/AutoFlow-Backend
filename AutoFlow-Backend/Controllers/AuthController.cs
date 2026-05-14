using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Auth;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Extensions;
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
    private readonly IIdentityService _identityService;

    public AuthController(
        IAuthService authService,
        IRegistrationService registrationService,
        IIdentityService identityService)
    {
        _authService = authService;
        _registrationService = registrationService;
        _identityService = identityService;
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

    /// <summary>
    /// [Authenticated] Change your own password. Works for all authenticated roles (Customer, Staff, Admin).
    /// </summary>
    /// <param name="request">Current and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success or failure message</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<bool>("Invalid user token."));

        var (succeeded, error) = await _identityService.ChangePasswordAsync(
            userId.Value.ToString(),
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);

        if (!succeeded)
            return BadRequest(ApiResponseFactory.Fail<bool>(error ?? "Failed to change password."));

        return Ok(ApiResponseFactory.Ok("Password changed successfully.", true));
    }
}