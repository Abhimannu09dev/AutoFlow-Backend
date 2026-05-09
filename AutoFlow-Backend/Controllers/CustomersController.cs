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
    /// [Admin, Staff] Create a new customer (with optional login account and vehicle)
    /// </summary>
    /// <param name="request">Customer details including FullName, Email, Phone, Address, CreateLoginAccount flag, and optional Vehicle details. When createLoginAccount is true, creates a linked user account. When vehicle is provided, creates a vehicle linked to the customer.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created customer details including ApplicationUserId when login account is created</returns>
    /// <response code="200">Customer created successfully</response>
    /// <response code="400">Validation error or creation failed</response>
    /// <response code="409">Email already exists (as user account or customer)</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status409Conflict)]
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
    /// [Admin, Staff] Get all customers registered in the system
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
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
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
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }
        return Ok(response);
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
        if (!response.IsSuccess)
            return BadRequest(response);

        return Ok(response);
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
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
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
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
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
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

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
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    private Guid? GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value is { } idStr && Guid.TryParse(idStr, out var userId) ? userId : null;
}

[ApiController]
[Route("api/customers/me")]
[Authorize(Roles = "Customer")]
[Tags("Customer Self-Service")]
public class CustomerHistoryController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerHistoryController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    private Guid? GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value is { } idStr && Guid.TryParse(idStr, out var userId) ? userId : null;

    /// <summary>
    /// [Customer] Get your own purchase history
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Your purchase history</returns>
    [HttpGet("purchases")]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<SaleResponse>>>> GetMyPurchases(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<List<SaleResponse>>("Invalid user token."));

        var response = await _customerService.GetMyPurchasesAsync(userId.Value, cancellationToken);
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// [Customer] Get your own service history (appointments)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Your service history</returns>
    [HttpGet("services")]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<AppointmentResponse>>>> GetMyServices(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<List<AppointmentResponse>>("Invalid user token."));

        var response = await _customerService.GetMyServicesAsync(userId.Value, cancellationToken);
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// [Customer] Get your own profile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Your profile details</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<CustomerResponseDto>("Invalid user token."));

        var response = await _customerService.GetMyProfileAsync(userId.Value, cancellationToken);
        if (!response.IsSuccess)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// [Customer] Update your own profile
    /// </summary>
    /// <param name="request">Updated profile details (FullName, Phone, Address - Email cannot be changed)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated profile details</returns>
    [HttpPatch("profile")]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CustomerResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> UpdateMyProfile(
        [FromBody] CustomerPatchDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<CustomerResponseDto>("Invalid user token."));

        var response = await _customerService.UpdateMyProfileAsync(userId.Value, request, cancellationToken);
        if (!response.IsSuccess)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }
}
