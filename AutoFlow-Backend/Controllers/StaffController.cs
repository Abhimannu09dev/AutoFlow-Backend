using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Staff;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StaffResponse>>> Create(
        [FromBody] CreateStaffRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _staffService.CreateAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StaffResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _staffService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StaffResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _staffService.GetByIdAsync(id, cancellationToken);
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
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

    [HttpDelete("{id:guid}")]
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
