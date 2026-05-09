using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize(Roles = "Admin")]
[Tags("Sales")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly IStaffRepository _staffRepository;

    public SalesController(ISaleService saleService, IStaffRepository staffRepository)
    {
        _saleService = saleService;
        _staffRepository = staffRepository;
    }

    /// <summary>
    /// [Staff] Record a new sale transaction. Requires an active Staff profile to create sales.
    /// </summary>
    /// <param name="request">Sale details (CustomerId, Items, PaymentMethod, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created sale with items</returns>
    [HttpPost]
    [Authorize(Roles = "Staff")]
    [ProducesResponseType(typeof(ApiResponse<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SaleResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var applicationUserId))
            return Unauthorized();

        var staff = await _staffRepository.GetActiveByApplicationUserIdAsync(applicationUserId, cancellationToken);
        if (staff is null)
            return Forbid();

        var result = await _saleService.CreateAsync(request, staff.Id, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// [Admin] Get all sales transactions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all sales</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _saleService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// [Admin] Get a sale by its ID
    /// </summary>
    /// <param name="id">Sale ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sale details with items</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SaleResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _saleService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// [Admin] Get all sales for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer's purchases</returns>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomerId(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _saleService.GetByCustomerIdAsync(customerId, cancellationToken);
        return Ok(result);
    }
}