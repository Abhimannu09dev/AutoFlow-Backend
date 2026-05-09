using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reports;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/reports/customers")]
[Authorize(Roles = "Admin,Staff")]
[Tags("Customer Reports")]
public class CustomerReportsController : ControllerBase
{
    private readonly ICustomerReportService _customerReportService;

    public CustomerReportsController(ICustomerReportService customerReportService)
    {
        _customerReportService = customerReportService;
    }

    /// <summary>
    /// [Admin, Staff] Get a list of top spending customers ordered by total spending amount
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customers ordered by total spending</returns>
    [HttpGet("top-spenders")]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerTopSpenderReportResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerTopSpenderReportResponse>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<CustomerTopSpenderReportResponse>>>> GetTopSpenders(
        CancellationToken cancellationToken)
    {
        var response = await _customerReportService.GetTopSpendersAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// [Admin, Staff] Get customers who have visited or made a purchase in the last 30 days
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active/regular customers</returns>
    [HttpGet("regular")]
    [ProducesResponseType(typeof(ApiResponse<List<RegularCustomerReportResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<RegularCustomerReportResponse>>>> GetRegularCustomers(
        CancellationToken cancellationToken)
    {
        var response = await _customerReportService.GetRegularCustomersAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// [Admin, Staff] Get all customers who have a pending or overdue credit balance
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customers with outstanding credit</returns>
    [HttpGet("pending-credit")]
    [ProducesResponseType(typeof(ApiResponse<List<PendingCreditCustomerReportResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PendingCreditCustomerReportResponse>>>> GetPendingCredit(
        CancellationToken cancellationToken)
    {
        var response = await _customerReportService.GetPendingCreditCustomersAsync(cancellationToken);
        return Ok(response);
    }
}