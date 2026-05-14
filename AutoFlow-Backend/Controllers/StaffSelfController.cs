using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Staff;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/staff/me")]
[Authorize(Roles = "Staff,Admin")]
[Tags("Staff Self-Service")]
public class StaffSelfController : BaseController
{
    private readonly IStaffSelfService _staffSelfService;

    public StaffSelfController(IStaffSelfService staffSelfService)
    {
        _staffSelfService = staffSelfService;
    }

    /// <summary>
    /// [Staff, Admin] Get your own profile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Your profile details</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StaffResponse>>> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<StaffResponse>("Invalid user token."));

        var response = await _staffSelfService.GetMyProfileAsync(userId.Value, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Staff, Admin] Update your own profile (FullName, Phone, Address)
    /// </summary>
    /// <param name="request">Updated profile details. Email, Position, and StaffCode cannot be changed here.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated profile details</returns>
    [HttpPatch("profile")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StaffResponse>>> UpdateMyProfile(
        [FromBody] StaffPatchDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<StaffResponse>("Invalid user token."));

        var response = await _staffSelfService.UpdateMyProfileAsync(userId.Value, request, cancellationToken);
        return response.ToActionResult();
    }
}
