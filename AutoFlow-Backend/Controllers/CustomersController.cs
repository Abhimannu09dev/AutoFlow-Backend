using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Admin,Staff")]
[Tags("Customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="request">Customer details (FullName, Email, Phone, Address)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created customer details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> Create(
        [FromBody] CustomerCreateDto request,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.CreateAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all customers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<CustomerResponseDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _customerService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get customer's purchase history
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer's purchases (sales)</returns>
    [HttpGet("{id:guid}/purchases")]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<SaleResponse>>>> GetPurchases(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.GetPurchasesAsync(id, cancellationToken);
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get customer's service history (appointments)
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer's appointments</returns>
    [HttpGet("{id:guid}/services")]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<AppointmentResponse>>>> GetServices(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.GetServicesAsync(id, cancellationToken);
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }
        return Ok(response);
    }

    /// <summary>
    /// Search customers by name or email
    /// </summary>
    /// <param name="query">Search query (name or email)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching customers</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerResponseDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<CustomerResponseDto>>>> Search(
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.SearchAsync(query, cancellationToken);
        if (!response.IsSuccess)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.GetByIdAsync(id, cancellationToken);
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// Update customer details
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="request">Updated customer details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated customer details</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> Update(
        Guid id,
        [FromBody] CustomerUpdateDto request,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.UpdateAsync(id, request, cancellationToken);
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Add a vehicle to customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="request">Vehicle details (VehicleNumber, Brand, Model, Year, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created vehicle details</returns>
    [HttpPost("{id:guid}/vehicles")]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VehicleResponseDto>>> AddVehicle(
        Guid id,
        [FromBody] VehicleCreateDto request,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.AddVehicleAsync(id, request, cancellationToken);
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return CreatedAtAction(nameof(GetVehicles), new { id }, response);
    }

    /// <summary>
    /// Get customer's vehicles
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer's vehicles</returns>
    [HttpGet("{id:guid}/vehicles")]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleResponseDto>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleResponseDto>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<VehicleResponseDto>>>> GetVehicles(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.GetVehiclesAsync(id, cancellationToken);
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }
}
