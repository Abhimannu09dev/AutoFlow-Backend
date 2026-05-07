using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Dashboard;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IAppDbContext _context;

    public DashboardService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var totalSalesCount = await _context.Sales
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalRevenue = await _context.Sales
            .AsNoTracking()
            .SumAsync(sale => (decimal?)sale.TotalAmount, cancellationToken) ?? 0m;

        var totalCustomersCount = await _context.Customers
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalStaffCount = await _context.Staff
            .AsNoTracking()
            .CountAsync(staff => staff.IsActive, cancellationToken);

        var lowStockParts = await _context.Parts
            .AsNoTracking()
            .Where(part => part.IsActive && part.StockQuantity < part.MinimumStockLevel)
            .OrderBy(part => part.StockQuantity)
            .ThenBy(part => part.PartName)
            .Select(part => new LowStockPartDashboardResponse
            {
                PartId = part.Id,
                PartName = part.PartName,
                PartNumber = part.PartNumber,
                StockQuantity = part.StockQuantity,
                MinimumStockLevel = part.MinimumStockLevel
            })
            .ToListAsync(cancellationToken);

        var dashboard = new DashboardResponse
        {
            TotalSalesCount = totalSalesCount,
            TotalRevenue = totalRevenue,
            TotalCustomersCount = totalCustomersCount,
            TotalStaffCount = totalStaffCount,
            LowStockParts = lowStockParts
        };

        return new ApiResponse<DashboardResponse>
        {
            Status = true,
            Message = "Dashboard data retrieved successfully.",
            Data = dashboard
        };
    }
}
