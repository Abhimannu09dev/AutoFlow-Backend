using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/vehicles")]
[Tags("Vehicles")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    private Guid? GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value is { } idStr && Guid.TryParse(idStr, out var userId) ? userId : null;

    private bool IsStaffOrAdmin() => User.IsInRole("Admin") || User.IsInRole("Staff");

    /// <summary>
    /// Create a new vehicle for the authenticated customer.
    /// Staff/Admin can create vehicles on behalf of customers using OwnerUserId.
    /// </summary>
    /// <param name="request">Vehicle details (VehicleNumber, Brand, Model, Year, Mileage, Color, VIN, OwnerUserId)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created vehicle details</returns>
    [HttpPost]
    [Authorize(Roles = "Customer,Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> Create(
        [FromBody] VehicleCreateDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isStaffOrAdmin = IsStaffOrAdmin();

        var response = await _vehicleService.CreateAsync(request, userId, isStaffOrAdmin, cancellationToken);
        if (!response.IsSuccess)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

/// <summary>
    /// Get all vehicles. Staff/Admin see all vehicles; Customers see only their own.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of vehicles</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleResponseDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<VehicleResponseDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isStaffOrAdmin = IsStaffOrAdmin();

        var response = await _vehicleService.GetAllAsync(userId, isStaffOrAdmin, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get vehicle by ID. Customers can only access their own vehicles.
    /// </summary>
    /// <param name="id">Vehicle ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vehicle details</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Customer,Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isStaffOrAdmin = IsStaffOrAdmin();

        var response = await _vehicleService.GetByIdAsync(id, userId, isStaffOrAdmin, cancellationToken);
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// Update a vehicle. Mileage can only be updated by Staff/Admin.
    /// </summary>
    /// <param name="id">Vehicle ID</param>
    /// <param name="request">Updated vehicle details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated vehicle details</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Customer,Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> Update(
        Guid id,
        [FromBody] VehicleUpdateDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isStaffOrAdmin = IsStaffOrAdmin();

        var response = await _vehicleService.UpdateAsync(id, request, userId, isStaffOrAdmin, cancellationToken);
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Delete a vehicle. Only the owner or Staff/Admin can delete.
    /// </summary>
    /// <param name="id">Vehicle ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Customer,Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isStaffOrAdmin = IsStaffOrAdmin();

        var response = await _vehicleService.DeleteAsync(id, userId, isStaffOrAdmin, cancellationToken);
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }
}