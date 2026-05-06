using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reports;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/reports/customers")]
[Authorize(Roles = "Admin,Staff")]
public class CustomerReportsController : ControllerBase
{
    private readonly ICustomerReportService _customerReportService;

    public CustomerReportsController(ICustomerReportService customerReportService)
    {
        _customerReportService = customerReportService;
    }

    [HttpGet("top-spenders")]
    public async Task<ActionResult<ApiResponse<List<CustomerTopSpenderReportResponse>>>> GetTopSpenders(
        CancellationToken cancellationToken)
    {
        var response = await _customerReportService.GetTopSpendersAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("regular")]
    public async Task<ActionResult<ApiResponse<List<RegularCustomerReportResponse>>>> GetRegularCustomers(
        CancellationToken cancellationToken)
    {
        var response = await _customerReportService.GetRegularCustomersAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("pending-credit")]
    public async Task<ActionResult<ApiResponse<List<PendingCreditCustomerReportResponse>>>> GetPendingCredit(
        CancellationToken cancellationToken)
    {
        var response = await _customerReportService.GetPendingCreditCustomersAsync(cancellationToken);
        return Ok(response);
    }
}
