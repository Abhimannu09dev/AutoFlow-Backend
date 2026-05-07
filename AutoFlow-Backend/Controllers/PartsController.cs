using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Parts;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/parts")]
[Authorize(Roles = "Admin,Staff")]
public class PartsController : ControllerBase
{
    private readonly IPartService _partService;

    public PartsController(IPartService partService)
    {
        _partService = partService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PartResponse>>> Create(
        [FromBody] CreatePartRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _partService.CreateAsync(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PartResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _partService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<PartResponse>>>> Search(
        [FromQuery] string? query,
        CancellationToken cancellationToken)
    {
        var response = await _partService.SearchAsync(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<ApiResponse<List<PartResponse>>>> GetLowStock(CancellationToken cancellationToken)
    {
        var response = await _partService.GetLowStockAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PartResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _partService.GetByIdAsync(id, cancellationToken);
        if (!response.Status)
            return NotFound(response);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PartResponse>>> Update(
    Guid id,
    [FromBody] UpdatePartRequest request,
    CancellationToken cancellationToken)
    {
        var response = await _partService.UpdateAsync(id, request, cancellationToken);
        if (!response.Status)
            return response.ErrorType == ErrorType.NotFound
                ? NotFound(response) : BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _partService.DeleteAsync(id, cancellationToken);
        if (!response.Status)
            return NotFound(response);
        return Ok(response);
    }
}