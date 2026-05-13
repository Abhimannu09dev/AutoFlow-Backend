using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Customers;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Admin,Staff")]
[Tags("Customers")]
public class CustomersController : BaseController
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// [Admin, Staff] Create a new customer (with optional login account and vehicle)
    /// </summary>
    /// <param name="request">Customer details including FullName, Email, Phone, Address, CreateLoginAccount flag, and optional Vehicle details. When createLoginAccount is true, creates a linked user account. When vehicle is provided, creates a vehicle linked to the customer.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created customer details including ApplicationUserId when login account is created</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> Create(
        [FromBody] CustomerCreateDto request,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.CreateAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return response.ToActionResult();

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    /// <summary>
    /// [Admin, Staff] Get all customers registered in the system
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of customers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CustomerResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<CustomerResponseDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResponse<CustomerResponseDto>>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _customerService.GetAllAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// [Admin, Staff] Get a specific customer's purchase history (sales transactions)
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
        return response.ToActionResult();
    }

    /// <summary>
    /// [Admin, Staff] Get a specific customer's service history (appointments)
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
        return response.ToActionResult();
    }

    /// <summary>
    /// [Admin, Staff] Search customers by name or email address
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
        return response.ToActionResult();
    }

    /// <summary>
    /// [Admin, Staff] Get a customer by their ID
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
        return response.ToActionResult();
    }

    /// <summary>
    /// [Admin, Staff] Update a customer's details (name, phone, address, etc.)
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
        return response.ToActionResult();
    }

    /// <summary>
    /// [Admin, Staff] Add a vehicle to a customer's profile
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
            return response.ToActionResult();

        return CreatedAtAction(nameof(GetVehicles), new { id }, response);
    }

    /// <summary>
    /// [Admin, Staff] Get all vehicles registered under a customer
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
        return response.ToActionResult();
    }
}
