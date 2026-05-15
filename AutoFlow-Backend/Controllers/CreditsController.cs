using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Credits;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/credits")]
[Authorize(Roles = "Admin,Staff")]
[Tags("Credits")]
public class CreditsController : BaseController
{
    private readonly ICreditService _creditService;

    public CreditsController(ICreditService creditService)
    {
        _creditService = creditService;
    }

    /// <summary>
    /// [Admin, Staff] Get detailed credit information for a specific sale, including payment history.
    /// </summary>
    /// <param name="saleId">Sale ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Credit details with payment history</returns>
    [HttpGet("{saleId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CreditDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreditDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreditDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CreditDetailResponse>>> GetDetails(
        Guid saleId,
        CancellationToken cancellationToken)
    {
        var result = await _creditService.GetCreditDetailsAsync(saleId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// [Staff] Record a payment against a credit sale, reducing the outstanding balance.
    /// </summary>
    /// <param name="saleId">Sale ID</param>
    /// <param name="request">Payment details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated credit summary</returns>
    [HttpPost("{saleId:guid}/payments")]
    [Authorize(Roles = "Staff")]
    [ProducesResponseType(typeof(ApiResponse<RecordCreditPaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecordCreditPaymentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RecordCreditPaymentResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RecordCreditPaymentResponse>>> RecordPayment(
        Guid saleId,
        [FromBody] RecordCreditPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _creditService.RecordPaymentAsync(saleId, request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// [Staff] Manually update the credit status (e.g., mark as Paid after manual reconciliation).
    /// </summary>
    /// <param name="saleId">Sale ID</param>
    /// <param name="request">New credit status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated credit status</returns>
    [HttpPatch("{saleId:guid}/status")]
    [Authorize(Roles = "Staff")]
    [ProducesResponseType(typeof(ApiResponse<UpdateCreditStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UpdateCreditStatusResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UpdateCreditStatusResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UpdateCreditStatusResponse>>> UpdateStatus(
        Guid saleId,
        [FromBody] UpdateCreditStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _creditService.UpdateStatusAsync(saleId, request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// [Admin, Staff] Send a credit payment reminder email to the customer for a specific sale.
    /// </summary>
    /// <param name="saleId">Sale ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Send result</returns>
    [HttpPost("{saleId:guid}/send-reminder")]
    [ProducesResponseType(typeof(ApiResponse<SendCreditReminderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SendCreditReminderResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SendCreditReminderResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SendCreditReminderResponse>>> SendReminder(
        Guid saleId,
        CancellationToken cancellationToken)
    {
        var result = await _creditService.SendReminderAsync(saleId, cancellationToken);
        return result.ToActionResult();
    }
}
