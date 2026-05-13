using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Extensions;
using AutoFlow_Backend.Application.DTOs.Parts;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/parts")]
[Authorize(Roles = "Admin,Staff")]
[Tags("Parts Inventory")]
public class PartsController : BaseController
{
    private readonly IPartService _partService;

    public PartsController(IPartService partService)
    {
        _partService = partService;
    }

    /// <summary>
    /// [Admin] Add a new part to the inventory
    /// </summary>
    /// <param name="request">Part details (PartName, PartNumber, UnitPrice, VendorId, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created part details</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PartResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PartResponse>>> Create(
        [FromBody] CreatePartRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _partService.CreateAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return response.ToActionResult();
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    /// <summary>
    /// [Admin, Staff] Get all parts in the inventory
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of parts</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PartResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PartResponse>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResponse<PartResponse>>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _partService.GetAllAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// [Admin, Staff] Search parts by name, part number, or category
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching parts</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<List<PartResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PartResponse>>>> Search(
        [FromQuery] string? query,
        CancellationToken cancellationToken)
    {
        var response = await _partService.SearchAsync(query, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// [Admin, Staff] Get all parts that are below the minimum stock threshold
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of low stock parts</returns>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<List<PartResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PartResponse>>>> GetLowStock(CancellationToken cancellationToken)
    {
        var response = await _partService.GetLowStockAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// [Admin, Staff] Get a specific part by its ID
    /// </summary>
    /// <param name="id">Part ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Part details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PartResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PartResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _partService.GetByIdAsync(id, cancellationToken);
        if (!response.IsSuccess)
            return response.ToActionResult();
        return Ok(response);
    }

    /// <summary>
    /// [Admin] Update part details (price, stock level, vendor, etc.)
    /// </summary>
    /// <param name="id">Part ID</param>
    /// <param name="request">Updated part details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated part details</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PartResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PartResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PartResponse>>> Update(
    Guid id,
    [FromBody] UpdatePartRequest request,
    CancellationToken cancellationToken)
    {
        var response = await _partService.UpdateAsync(id, request, cancellationToken);
        if (!response.IsSuccess)
            return response.ToActionResult();
        return Ok(response);
    }

    /// <summary>
    /// [Admin] Remove a part from the inventory (hard delete)
    /// </summary>
    /// <param name="id">Part ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _partService.DeleteAsync(id, cancellationToken);
        if (!response.IsSuccess)
            return response.ToActionResult();
        return Ok(response);
    }
}