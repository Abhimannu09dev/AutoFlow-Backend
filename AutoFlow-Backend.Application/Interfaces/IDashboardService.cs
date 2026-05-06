using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Dashboard;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<DashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
}
