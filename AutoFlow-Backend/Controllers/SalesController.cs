using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize(Roles = "Admin,Staff")]
[Tags("Sales")]
public class SalesController : BaseController
{
    private readonly ISaleService _saleService;
    private readonly IStaffService _staffService;

    public SalesController(ISaleService saleService, IStaffService staffService)
    {
        _saleService = saleService;
        _staffService = staffService;
    }

    /// <summary>
    /// [Staff] Record a new sale transaction and auto-send a formal invoice to the customer. Requires an active Staff profile.
    /// </summary>
    /// <param name="request">Sale details (CustomerId, Items, PaymentMethod, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created sale with items</returns>
    [HttpPost]
    [Authorize(Roles = "Staff")]
    [ProducesResponseType(typeof(ApiResponse<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SaleResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SaleResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<SaleResponse>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<SaleResponse>>> Create(
        [FromBody] CreateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized(ApiResponseFactory.Fail<SaleResponse>("Invalid user token."));

        var staffId = await _staffService.GetStaffIdByApplicationUserIdAsync(userId.Value, cancellationToken);
        if (staffId is null)
            return Forbid();

        var result = await _saleService.CreateAsync(request, staffId.Value, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// [Admin, Staff] Get all sales transactions
    /// </summary>
    /// <param name="request">Pagination (page, pageSize) and sort parameters (sortBy, sortDir). Defaults: page=1, pageSize=20 (max 100), sortBy=saleDate, sortDir=desc.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of sales</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<SaleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<SaleResponse>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResponse<SaleResponse>>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _saleService.GetAllAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// [Admin, Staff] Get a sale by its ID
    /// </summary>
    /// <param name="id">Sale ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sale details with items</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SaleResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SaleResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _saleService.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// [Admin, Staff] Get all sales for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer's purchases</returns>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<List<SaleResponse>>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetByCustomerId(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _saleService.GetByCustomerIdAsync(customerId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// [Staff, Admin] Send or resend a formal invoice email to the customer for a specific sale. Can be used to resend if the previous attempt failed.
    /// </summary>
    /// <param name="id">Sale ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Invoice send result</returns>
    [HttpPost("{id:guid}/send-invoice")]
    [Authorize(Roles = "Staff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<SendInvoiceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SendInvoiceResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SendInvoiceResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SendInvoiceResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SendInvoiceResponse>>> SendInvoice(Guid id, CancellationToken cancellationToken)
    {
        var result = await _saleService.SendInvoiceAsync(id, cancellationToken);
        return result.ToActionResult();
    }
}