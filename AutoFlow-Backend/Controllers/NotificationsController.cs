using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "Admin")]
[Tags("Notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// [Admin] Send low stock alert notifications for parts below minimum threshold
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Notification result for parts below minimum stock</returns>
    [HttpPost("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendLowStockAlert(CancellationToken cancellationToken)
    {
        var result = await _notificationService.SendLowStockAlertAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// [Admin] Send credit overdue reminders to customers with outstanding balances
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Notification result for overdue credits</returns>
    [HttpPost("credit-overdue")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendCreditOverdueReminders(CancellationToken cancellationToken)
    {
        var result = await _notificationService.SendCreditOverdueRemindersAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}