using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Dashboard;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<DashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ActivityStreamItemResponse>>> GetActivityStreamAsync(int limit, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<RevenueTrendPointResponse>>> GetRevenueTrendAsync(RevenueTrendRange range, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<FastMovingInventoryResponse>>> GetFastMovingInventoryAsync(int limit, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PriorityAlertResponse>>> GetPriorityAlertsAsync(int limit, CancellationToken cancellationToken = default);
}
