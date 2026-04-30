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
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> Create(
        [FromBody] CustomerCreateDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _customerService.CreateAsync(request, cancellationToken);
            if (!response.Status)
            {
                return BadRequest(response);
            }

            return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<CustomerResponseDto>
            {
                Status = false,
                Message = "An unexpected error occurred.",
                Data = null
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CustomerResponseDto>>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _customerService.GetAllAsync(cancellationToken);
            return Ok(response);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<CustomerResponseDto>>
            {
                Status = false,
                Message = "An unexpected error occurred.",
                Data = null
            });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _customerService.GetByIdAsync(id, cancellationToken);
            return response.Status ? Ok(response) : NotFound(response);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<CustomerResponseDto>
            {
                Status = false,
                Message = "An unexpected error occurred.",
                Data = null
            });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> Update(
        int id,
        [FromBody] CustomerUpdateDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _customerService.UpdateAsync(id, request, cancellationToken);
            if (!response.Status)
            {
                return string.Equals(response.Message, "Customer not found.", StringComparison.Ordinal)
                    ? NotFound(response)
                    : BadRequest(response);
            }

            return Ok(response);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<CustomerResponseDto>
            {
                Status = false,
                Message = "An unexpected error occurred.",
                Data = null
            });
        }
    }
}