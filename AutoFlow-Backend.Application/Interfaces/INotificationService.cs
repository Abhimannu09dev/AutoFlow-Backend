using AutoFlow_Backend.Application.Common;

namespace AutoFlow_Backend.Application.Interfaces;

public interface INotificationService
{
    Task<ApiResponse<bool>> SendLowStockAlertAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SendCreditOverdueRemindersAsync(CancellationToken cancellationToken = default);
}