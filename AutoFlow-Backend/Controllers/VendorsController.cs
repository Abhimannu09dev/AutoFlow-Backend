using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Vendors;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/vendors")]
[Authorize(Roles = "Admin")]
[Tags("Vendors")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorsController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    /// <summary>
    /// Create a new vendor/supplier
    /// </summary>
    /// <param name="request">Vendor details (VendorName, ContactPerson, Email, Phone)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created vendor details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VendorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VendorResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> Create(
        [FromBody] CreateVendorRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _vendorService.CreateAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return response.ErrorType == ErrorType.NotFound ? NotFound(response) : BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// Get all vendors
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all vendors</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<VendorResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<VendorResponse>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await _vendorService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get vendor by ID
    /// </summary>
    /// <param name="id">Vendor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vendor details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VendorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VendorResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _vendorService.GetByIdAsync(id, cancellationToken);
        if (!response.IsSuccess)
            return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// Update vendor details
    /// </summary>
    /// <param name="id">Vendor ID</param>
    /// <param name="request">Updated vendor details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated vendor details</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VendorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VendorResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VendorResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> Update(
        Guid id,
        [FromBody] UpdateVendorRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _vendorService.UpdateAsync(id, request, cancellationToken);
        if (!response.IsSuccess)
            return response.ErrorType == ErrorType.NotFound ? NotFound(response) : BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// Delete a vendor
    /// </summary>
    /// <param name="id">Vendor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _vendorService.DeleteAsync(id, cancellationToken);
        if (!response.IsSuccess)
            return NotFound(response);
        return Ok(response);
    }

    /// <summary>
    /// Search vendors by name or contact person
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching vendors</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<List<VendorResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<VendorResponse>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<VendorResponse>>>> Search(
        [FromQuery] string? query,
        CancellationToken cancellationToken)
    {
        var response = await _vendorService.SearchAsync(query, cancellationToken);
        if (!response.IsSuccess)
            return BadRequest(response);
        return Ok(response);
    }
}