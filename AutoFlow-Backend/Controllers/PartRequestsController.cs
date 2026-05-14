using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PartRequests;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/part-requests")]
[Authorize(Roles = "Customer,Admin,Staff")]
[Tags("Part Requests")]
public class PartRequestsController : BaseController
{
    private readonly IPartRequestService _partRequestService;

    public PartRequestsController(IPartRequestService partRequestService)
    {
        _partRequestService = partRequestService;
    }

    /// <summary>
    /// [Customer, Staff, Admin] Create a part request for items not in inventory. Customers request for themselves; Staff/Admin can request for any customer.
    /// </summary>
    /// <param name="request">Part request details (CustomerId optional for customers, PartName, Quantity)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created part request details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PartRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PartRequestResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PartRequestResponse>>> Create(
        [FromBody] CreatePartRequestRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isStaffOrAdmin = IsStaffOrAdmin();

        var response = await _partRequestService.CreateAsync(request, userId, isStaffOrAdmin, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Customer, Staff, Admin] Get part requests. Customers see only their own; Staff/Admin see all.
    /// </summary>
    /// <param name="request">Pagination (page, pageSize) and sort parameters (sortBy, sortDir). Defaults: page=1, pageSize=20 (max 100), sortBy=createdAt, sortDir=desc.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of part requests</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PartRequestResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<PartRequestResponse>>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isStaffOrAdmin = IsStaffOrAdmin();

        var response = await _partRequestService.GetAllAsync(request, userId, isStaffOrAdmin, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Admin, Staff] Update status for a part request.
    /// Status values supported by this endpoint: Pending, Done, Rejected.
    /// Done is mapped to Fulfilled in the domain enum.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(typeof(ApiResponse<PartRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PartRequestResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PartRequestResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PartRequestResponse>>> UpdateStatus(
        Guid id,
        [FromBody] UpdatePartRequestStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _partRequestService.UpdateStatusAsync(id, request, cancellationToken);
        return response.ToActionResult();
    }
}
