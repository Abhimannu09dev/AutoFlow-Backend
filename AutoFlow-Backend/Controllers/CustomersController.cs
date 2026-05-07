using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Admin,Staff")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> Create(
        [FromBody] CustomerCreateDto request,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.CreateAsync(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CustomerResponseDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _customerService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.GetByIdAsync(id, cancellationToken);
        if (!response.Status)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> Update(
        Guid id,
        [FromBody] CustomerUpdateDto request,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.UpdateAsync(id, request, cancellationToken);
        if (!response.Status)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("{id:guid}/vehicles")]
    public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> AddVehicle(
        Guid id,
        [FromBody] VehicleCreateDto request,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.AddVehicleAsync(id, request, cancellationToken);
        if (!response.Status)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return CreatedAtAction(nameof(GetVehicles), new { id }, response);
    }

    [HttpGet("{id:guid}/vehicles")]
    public async Task<ActionResult<ApiResponse<List<VehicleResponseDto>>>> GetVehicles(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.GetVehiclesAsync(id, cancellationToken);
        if (!response.Status)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }
}
