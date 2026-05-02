using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid request." : e.ErrorMessage)
                .ToList();

            var badRequestResponse = new APIResponse
            {
                Success = false,
                Message = "Validation failed.",
                Data = null,
                StatusCode = StatusCodes.Status400BadRequest,
                Errors = modelErrors
            };

            return BadRequest(badRequestResponse);
        }

        var response = await _customerService.CreateAsync(request, cancellationToken);
        if (!response.Success)
        {
            return BadRequest(response);
        }

        var createdCustomer = response.Data as CustomerResponseDto;
        return CreatedAtAction(nameof(GetById), new { id = createdCustomer?.Id }, response);
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
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid request." : e.ErrorMessage)
                .ToList();

            var badRequestResponse = new APIResponse
            {
                Success = false,
                Message = "Validation failed.",
                Data = null,
                StatusCode = StatusCodes.Status400BadRequest,
                Errors = modelErrors
            };

            return BadRequest(badRequestResponse);
        }

        var response = await _customerService.UpdateAsync(id, request, cancellationToken);
        return StatusCode(response.StatusCode, response);
    }
}