using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Dashboard;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;

namespace AutoFlow_Backend.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IReportQueryRepository _reportQueryRepository;

    public DashboardService(IReportQueryRepository reportQueryRepository)
    {
        _reportQueryRepository = reportQueryRepository;
    }

    public async Task<ApiResponse<DashboardResponse>> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        var totalSalesCount = await _reportQueryRepository.CountSalesAsync(cancellationToken);
        var totalRevenue = await _reportQueryRepository.SumSalesRevenueAsync(cancellationToken);
        var totalCustomersCount = await _reportQueryRepository.CountCustomersAsync(cancellationToken);
        var totalStaffCount = await _reportQueryRepository.CountActiveStaffAsync(cancellationToken);
        var lowStockParts = await _reportQueryRepository.GetLowStockPartsAsync(cancellationToken);

        var dashboard = new DashboardResponse
        {
            TotalSalesCount = totalSalesCount,
            TotalRevenue = totalRevenue,
            TotalCustomersCount = totalCustomersCount,
            TotalStaffCount = totalStaffCount,
            LowStockParts = lowStockParts.Select(part => new LowStockPartDashboardResponse
            {
                PartId = part.Id,
                PartName = part.PartName,
                PartNumber = part.PartNumber,
                StockQuantity = part.StockQuantity,
                MinimumStockLevel = part.MinimumStockLevel
            }).ToList()
        };

        return ApiResponseFactory.Ok("Dashboard data retrieved successfully.", dashboard);
    }
}