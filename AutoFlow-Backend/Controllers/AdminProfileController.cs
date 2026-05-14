using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Admin;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[Tags("Admin Profile")]
public class AdminProfileController : BaseController
{
    private readonly IAdminProfileService _adminProfileService;

    public AdminProfileController(IAdminProfileService adminProfileService)
    {
        _adminProfileService = adminProfileService;
    }

    /// <summary>
    /// [Admin] Get currently authenticated admin profile.
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<AdminProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminProfileResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AdminProfileResponse>>> GetProfile(CancellationToken cancellationToken)
    {
        var response = await _adminProfileService.GetProfileAsync(GetUserId(), cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Admin] Update currently authenticated admin profile.
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<AdminProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AdminProfileResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AdminProfileResponse>>> UpdateProfile(
        [FromBody] UpdateAdminProfileRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _adminProfileService.UpdateProfileAsync(GetUserId(), request, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Admin] Change currently authenticated admin password.
    /// </summary>
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(
        [FromBody] ChangeAdminPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _adminProfileService.ChangePasswordAsync(GetUserId(), request, cancellationToken);
        return response.ToActionResult();
    }
}
