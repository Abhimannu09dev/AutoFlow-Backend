using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PurchaseInvoices;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/purchase-invoices")]
[Authorize(Roles = "Admin")]
[Tags("Purchase Invoices")]
public class PurchaseInvoicesController : BaseController
{
    private readonly IPurchaseInvoiceService _purchaseInvoiceService;

    public PurchaseInvoicesController(IPurchaseInvoiceService purchaseInvoiceService)
    {
        _purchaseInvoiceService = purchaseInvoiceService;
    }

    /// <summary>
    /// [Admin] Create a purchase invoice from a vendor. Records inventory purchase and updates stock.
    /// </summary>
    /// <param name="request">Invoice details (VendorId, Items, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created invoice with items</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PurchaseInvoiceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseInvoiceResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var staffIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (staffIdClaim is null || !Guid.TryParse(staffIdClaim, out var staffId))
            return Unauthorized();

        var result = await _purchaseInvoiceService.CreateAsync(request, staffId, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// [Admin] Get all purchase invoices
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all purchase invoices</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PurchaseInvoiceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _purchaseInvoiceService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// [Admin] Get a purchase invoice by its ID
    /// </summary>
    /// <param name="id">Invoice ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Invoice details with items</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseInvoiceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseInvoiceResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _purchaseInvoiceService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// [Admin] Get all purchase invoices for a specific vendor
    /// </summary>
    /// <param name="vendorId">Vendor ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of invoices from specific vendor</returns>
    [HttpGet("vendor/{vendorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<PurchaseInvoiceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByVendorId(Guid vendorId, CancellationToken cancellationToken)
    {
        var result = await _purchaseInvoiceService.GetByVendorIdAsync(vendorId, cancellationToken);
        return Ok(result);
    }
}