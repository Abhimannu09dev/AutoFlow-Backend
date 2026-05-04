using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PartRequests;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/part-requests")]
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
