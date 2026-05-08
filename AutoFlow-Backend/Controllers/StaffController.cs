using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Staff;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = "Admin")]
[Tags("Staff")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    /// <summary>
    /// [Admin] Create a new staff member with user account
    /// </summary>
    /// <param name="request">Staff details including FullName, Email, Password, Position</param>
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
        if (!response.IsSuccess)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    /// <summary>
    /// [Admin] Get all active staff members
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all active staff members</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<StaffResponse>>), StatusCodes.Status200OK)]
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
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
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
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
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
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }
}
