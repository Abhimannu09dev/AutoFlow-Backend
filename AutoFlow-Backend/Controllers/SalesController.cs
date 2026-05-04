using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoFlow_Backend.API.Controllers;

[ApiController]
[Route("api/sales")]
[Authorize(Roles = "Admin,Staff")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var staffIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (staffIdClaim is null || !Guid.TryParse(staffIdClaim, out var staffId))
            return Unauthorized();

        var result = await _saleService.CreateAsync(request, staffId, cancellationToken);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _saleService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _saleService.GetByIdAsync(id, cancellationToken);
        return result.Status ? Ok(result) : NotFound(result);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomerId(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _saleService.GetByCustomerIdAsync(customerId, cancellationToken);
        return Ok(result);
    }
}