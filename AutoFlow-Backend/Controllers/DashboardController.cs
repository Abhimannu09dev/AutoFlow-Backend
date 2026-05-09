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
public class DashboardController : ControllerBase
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
}