using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PartRequests;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/part-requests")]
[Authorize(Roles = "Customer,Admin,Staff")]
public class PartRequestsController : ControllerBase
{
    private readonly IPartRequestService _partRequestService;

    public PartRequestsController(IPartRequestService partRequestService)
    {
        _partRequestService = partRequestService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PartRequestResponse>>> Create(
        [FromBody] CreatePartRequestRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _partRequestService.CreateAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PartRequestResponse>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await _partRequestService.GetAllAsync(cancellationToken);
        return Ok(response);
    }
}