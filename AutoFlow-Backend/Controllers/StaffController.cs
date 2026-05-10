using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Staff;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = "Admin")]
[Tags("Staff")]
public class StaffController : BaseController
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    /// <summary>
    /// [Admin] Create a new staff member with user account
    /// </summary>
    /// <param name="request">Staff details including FullName, Email, Password, Role (optional - defaults to Staff)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created staff details with user account</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<StaffResponse>>> Create(
        [FromBody] CreateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _staffService.CreateAsync(request, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Admin] Get all active staff members
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all active staff members</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<StaffResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<StaffResponse>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<StaffResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _staffService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// [Admin] Get a staff member by ID
    /// </summary>
    /// <param name="id">Staff ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Staff member details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StaffResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _staffService.GetByIdAsync(id, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Admin] Update staff member details (position, name, etc.)
    /// </summary>
    /// <param name="id">Staff ID</param>
    /// <param name="request">Updated staff details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated staff details</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StaffResponse>>> Update(
        Guid id,
        [FromBody] UpdateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _staffService.UpdateAsync(id, request, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Admin] Deactivate a staff member (soft delete). Staff can no longer log in.
    /// </summary>
    /// <param name="id">Staff ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deactivation confirmation</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _staffService.DeactivateAsync(id, cancellationToken);
        return response.ToActionResult();
    }
}
