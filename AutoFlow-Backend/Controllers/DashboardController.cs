using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Dashboard;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin")]
[Tags("Dashboard")]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// [Admin] Get an overview of key business metrics: total sales, revenue, customer count, appointments, and inventory status
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard statistics (sales count, revenue, customers, etc.)</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<DashboardResponse>>> Get(CancellationToken cancellationToken)
    {
        var response = await _dashboardService.GetDashboardAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// [Admin] Get recent dashboard activity stream.
    /// </summary>
    [HttpGet("activity-stream")]
    [ProducesResponseType(typeof(ApiResponse<List<ActivityStreamItemResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ActivityStreamItemResponse>>>> GetActivityStream(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _dashboardService.GetActivityStreamAsync(limit, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// [Admin] Get revenue trend by range: daily, weekly, monthly.
    /// </summary>
    [HttpGet("revenue-trend")]
    [ProducesResponseType(typeof(ApiResponse<List<RevenueTrendPointResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<RevenueTrendPointResponse>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<RevenueTrendPointResponse>>>> GetRevenueTrend(
        [FromQuery] string range = "daily",
        CancellationToken cancellationToken = default)
    {
        var response = await _dashboardService.GetRevenueTrendAsync(range, cancellationToken);
        return response.IsSuccess ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// [Admin] Get fast moving inventory based on completed sales.
    /// </summary>
    [HttpGet("fast-moving-inventory")]
    [ProducesResponseType(typeof(ApiResponse<List<FastMovingInventoryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<FastMovingInventoryResponse>>>> GetFastMovingInventory(
        [FromQuery] int limit = 5,
        CancellationToken cancellationToken = default)
    {
        var response = await _dashboardService.GetFastMovingInventoryAsync(limit, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// [Admin] Get priority alerts for operational attention.
    /// </summary>
    [HttpGet("priority-alerts")]
    [ProducesResponseType(typeof(ApiResponse<List<PriorityAlertResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PriorityAlertResponse>>>> GetPriorityAlerts(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _dashboardService.GetPriorityAlertsAsync(limit, cancellationToken);
        return Ok(response);
    }
}
