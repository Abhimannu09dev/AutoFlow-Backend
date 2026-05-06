using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "Admin")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("low-stock")]
    public async Task<IActionResult> SendLowStockAlert(CancellationToken cancellationToken)
    {
        var result = await _notificationService.SendLowStockAlertAsync(cancellationToken);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("credit-overdue")]
    public async Task<IActionResult> SendCreditOverdueReminders(CancellationToken cancellationToken)
    {
        var result = await _notificationService.SendCreditOverdueRemindersAsync(cancellationToken);
        return result.Status ? Ok(result) : BadRequest(result);
    }
}