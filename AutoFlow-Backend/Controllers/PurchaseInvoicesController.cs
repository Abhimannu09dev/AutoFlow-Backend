using AutoFlow_Backend.Application.DTOs.PurchaseInvoices;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoFlow_Backend.API.Controllers;

[ApiController]
[Route("api/purchase-invoices")]
[Authorize(Roles = "Admin")]
public class PurchaseInvoicesController : ControllerBase
{
    private readonly IPurchaseInvoiceService _purchaseInvoiceService;

    public PurchaseInvoicesController(IPurchaseInvoiceService purchaseInvoiceService)
    {
        _purchaseInvoiceService = purchaseInvoiceService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var staffIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (staffIdClaim is null || !Guid.TryParse(staffIdClaim, out var staffId))
            return Unauthorized();

        var result = await _purchaseInvoiceService.CreateAsync(request, staffId, cancellationToken);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _purchaseInvoiceService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _purchaseInvoiceService.GetByIdAsync(id, cancellationToken);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpGet("vendor/{vendorId:guid}")]
    public async Task<IActionResult> GetByVendorId(Guid vendorId, CancellationToken cancellationToken)
    {
        var result = await _purchaseInvoiceService.GetByVendorIdAsync(vendorId, cancellationToken);
        return Ok(result);
    }
}