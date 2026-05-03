using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<ActionResult<APIResponse>> Create(
        [FromBody] CustomerCreateDto request,
        CancellationToken cancellationToken)
    {
        // Model state validation handled globally via InvalidModelStateResponseFactory
        var response = await _customerService.CreateAsync(request, cancellationToken);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, response);
        }

        var createdCustomer = response.Data as CustomerResponseDto;
        if (response.StatusCode == StatusCodes.Status201Created)
        {
            return CreatedAtAction(nameof(GetById), new { id = createdCustomer?.Id }, response);
        }

        return StatusCode(response.StatusCode, response);
    }

    [HttpGet]
    public async Task<ActionResult<APIResponse>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _customerService.GetAllAsync(cancellationToken);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<APIResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.GetByIdAsync(id, cancellationToken);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<APIResponse>> Update(
        Guid id,
        [FromBody] CustomerUpdateDto request,
        CancellationToken cancellationToken)
    {
        // Model state validation handled globally via InvalidModelStateResponseFactory
        var response = await _customerService.UpdateAsync(id, request, cancellationToken);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{id:guid}/vehicles")]
    public async Task<ActionResult<APIResponse>> AddVehicle(
        Guid id,
        [FromBody] VehicleCreateDto request,
        CancellationToken cancellationToken)
    {
        // Model state validation handled globally via InvalidModelStateResponseFactory
        var response = await _customerService.AddVehicleAsync(id, request, cancellationToken);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id:guid}/vehicles")]
    public async Task<ActionResult<APIResponse>> GetVehicles(Guid id, CancellationToken cancellationToken)
    {
        var response = await _customerService.GetVehiclesAsync(id, cancellationToken);
        return StatusCode(response.StatusCode, response);
    }
}