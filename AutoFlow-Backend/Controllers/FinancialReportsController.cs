using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/reports/financial")]
[Authorize(Roles = "Admin")]
public class FinancialReportsController : ControllerBase
{
    private readonly IFinancialReportService _financialReportService;

    public FinancialReportsController(IFinancialReportService financialReportService)
    {
        _financialReportService = financialReportService;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily(
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        var result = await _financialReportService.GetDailyReportAsync(date, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var result = await _financialReportService.GetMonthlyReportAsync(year, month, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("yearly")]
    public async Task<IActionResult> GetYearly(
        [FromQuery] int year,
        CancellationToken cancellationToken)
    {
        var result = await _financialReportService.GetYearlyReportAsync(year, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}