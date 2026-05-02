using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Vendors;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/vendors")]
[Authorize(Roles = "Admin")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorsController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> Create(
        [FromBody] CreateVendorRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _vendorService.CreateAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<VendorResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _vendorService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _vendorService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> Update(
        Guid id,
        [FromBody] UpdateVendorRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _vendorService.UpdateAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _vendorService.DeleteAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<VendorResponse>>>> Search(
        [FromQuery] string? query,
        CancellationToken cancellationToken)
    {
        var response = await _vendorService.SearchAsync(query, cancellationToken);
        return Ok(response);
    }
}
