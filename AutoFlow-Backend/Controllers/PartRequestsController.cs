using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PartRequests;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/part-requests")]
[Authorize(Roles = "Customer,Admin,Staff")]
[Tags("Part Requests")]
public class PartRequestsController : ControllerBase
{
    private readonly IPartRequestService _partRequestService;

    public PartRequestsController(IPartRequestService partRequestService)
    {
        _partRequestService = partRequestService;
    }

    /// <summary>
    /// [Customer, Staff, Admin] Create a part request for items not in inventory. Customers request for themselves; Staff/Admin can request for any customer.
    /// </summary>
    /// <param name="request">Part request details (CustomerId, PartName, Quantity)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created part request details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PartRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PartRequestResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PartRequestResponse>>> Create(
        [FromBody] CreatePartRequestRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _partRequestService.CreateAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// [Customer, Staff, Admin] Get all part requests. Customers see only their own; Staff/Admin see all.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all part requests</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PartRequestResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PartRequestResponse>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await _partRequestService.GetAllAsync(cancellationToken);
        return Ok(response);
    }
}