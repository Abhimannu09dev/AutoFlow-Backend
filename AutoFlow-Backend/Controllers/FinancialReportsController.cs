using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reports;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/reports/financial")]
[Authorize(Roles = "Admin")]
[Tags("Financial Reports")]
public class FinancialReportsController : ControllerBase
{
    private readonly IFinancialReportService _financialReportService;

    public FinancialReportsController(IFinancialReportService financialReportService)
    {
        _financialReportService = financialReportService;
    }

    /// <summary>
    /// Get daily financial report
    /// </summary>
    /// <param name="date">Date for the report</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Daily revenue, profit, and sales breakdown</returns>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(ApiResponse<FinancialReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FinancialReportResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDaily(
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        var result = await _financialReportService.GetDailyReportAsync(date, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get monthly financial report
    /// </summary>
    /// <param name="year">Year (e.g., 2024)</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Monthly summary with daily breakdown</returns>
    [HttpGet("monthly")]
    [ProducesResponseType(typeof(ApiResponse<FinancialReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FinancialReportResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMonthly(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var result = await _financialReportService.GetMonthlyReportAsync(year, month, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get yearly financial report
    /// </summary>
    /// <param name="year">Year (e.g., 2024)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Yearly summary with monthly breakdown</returns>
    [HttpGet("yearly")]
    [ProducesResponseType(typeof(ApiResponse<FinancialReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FinancialReportResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetYearly(
        [FromQuery] int year,
        CancellationToken cancellationToken)
    {
        var result = await _financialReportService.GetYearlyReportAsync(year, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}