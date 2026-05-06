using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Dashboard;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardResponse>>> Get(CancellationToken cancellationToken)
    {
        var response = await _dashboardService.GetDashboardAsync(cancellationToken);
        return Ok(response);
    }
}
