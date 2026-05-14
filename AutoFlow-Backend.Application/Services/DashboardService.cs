using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Dashboard;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using System.Globalization;

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
        var utcToday = AsUtcDate(DateTime.UtcNow);
        var monthStart = new DateTime(utcToday.Year, utcToday.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);
        var yearStart = new DateTime(utcToday.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = yearStart.AddYears(1);

        var totalSalesCount = await _reportQueryRepository.CountSalesAsync(cancellationToken);
        var completedSalesCount = await _reportQueryRepository.CountCompletedSalesAsync(cancellationToken);
        var totalRevenue = await _reportQueryRepository.SumSalesRevenueAsync(cancellationToken);
        var todayRevenue = await _reportQueryRepository.SumCompletedSalesRevenueByDateRangeAsync(
            utcToday,
            utcToday.AddDays(1),
            cancellationToken);
        var monthlyRevenue = await _reportQueryRepository.SumCompletedSalesRevenueByDateRangeAsync(
            monthStart,
            monthEnd,
            cancellationToken);
        var yearlyRevenue = await _reportQueryRepository.SumCompletedSalesRevenueByDateRangeAsync(
            yearStart,
            yearEnd,
            cancellationToken);
        var totalCustomersCount = await _reportQueryRepository.CountCustomersAsync(cancellationToken);
        var totalStaffCount = await _reportQueryRepository.CountActiveStaffAsync(cancellationToken);
        var totalVendorsCount = await _reportQueryRepository.CountActiveVendorsAsync(cancellationToken);
        var totalPartsCount = await _reportQueryRepository.CountActivePartsAsync(cancellationToken);
        var totalPurchaseInvoicesCount = await _reportQueryRepository.CountPurchaseInvoicesAsync(cancellationToken);
        var pendingInvoicesCount = await _reportQueryRepository.CountPendingPurchaseInvoicesAsync(cancellationToken);
        var totalAppointmentsCount = await _reportQueryRepository.CountAppointmentsAsync(cancellationToken);
        var pendingAppointmentsCount = await _reportQueryRepository.CountPendingAppointmentsAsync(cancellationToken);
        var pendingPartRequestsCount = await _reportQueryRepository.CountPendingPartRequestsAsync(cancellationToken);
        var totalReviewsCount = await _reportQueryRepository.CountReviewsAsync(cancellationToken);
        var averageReviewRating = await _reportQueryRepository.GetAverageReviewRatingAsync(cancellationToken);
        var totalInventoryValue = await _reportQueryRepository.SumActiveInventoryCostValueAsync(cancellationToken);
        var lowStockParts = await _reportQueryRepository.GetLowStockPartsAsync(cancellationToken);

        var dashboard = new DashboardResponse
        {
            TotalSalesCount = totalSalesCount,
            CompletedSalesCount = completedSalesCount,
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue,
            MonthlyRevenue = monthlyRevenue,
            YearlyRevenue = yearlyRevenue,
            TotalCustomersCount = totalCustomersCount,
            TotalStaffCount = totalStaffCount,
            TotalVendorsCount = totalVendorsCount,
            TotalPartsCount = totalPartsCount,
            TotalPurchaseInvoicesCount = totalPurchaseInvoicesCount,
            PendingInvoicesCount = pendingInvoicesCount,
            TotalAppointmentsCount = totalAppointmentsCount,
            PendingAppointmentsCount = pendingAppointmentsCount,
            PendingPartRequestsCount = pendingPartRequestsCount,
            TotalReviewsCount = totalReviewsCount,
            AverageReviewRating = Math.Round(averageReviewRating, 2, MidpointRounding.AwayFromZero),
            LowStockPartsCount = lowStockParts.Count,
            TotalInventoryValue = totalInventoryValue,
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

    public async Task<ApiResponse<List<ActivityStreamItemResponse>>> GetActivityStreamAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var rows = await _reportQueryRepository.GetDashboardActivityStreamAsync(limit, cancellationToken);

        var response = rows.Select(item => new ActivityStreamItemResponse
        {
            Id = item.Id,
            Type = item.Type,
            Title = item.Title,
            Description = item.Description,
            CreatedAt = item.CreatedAt,
            ActorName = item.ActorName
        }).ToList();

        return ApiResponseFactory.Ok("Dashboard activity stream retrieved successfully.", response);
    }

    public async Task<ApiResponse<List<RevenueTrendPointResponse>>> GetRevenueTrendAsync(
        RevenueTrendRange range,
        CancellationToken cancellationToken = default)
    {
        var points = range switch
        {
            RevenueTrendRange.Daily => await BuildDailyTrendAsync(cancellationToken),
            RevenueTrendRange.Weekly => await BuildWeeklyTrendAsync(cancellationToken),
            RevenueTrendRange.Monthly => await BuildMonthlyTrendAsync(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(range), range, "Unsupported revenue trend range.")
        };

        return ApiResponseFactory.Ok("Dashboard revenue trend retrieved successfully.", points);
    }

    public async Task<ApiResponse<List<FastMovingInventoryResponse>>> GetFastMovingInventoryAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var rows = await _reportQueryRepository.GetFastMovingInventoryAsync(limit, cancellationToken);

        var response = rows.Select(row => new FastMovingInventoryResponse
        {
            PartId = row.PartId,
            PartName = row.PartName,
            PartNumber = row.PartNumber,
            SoldQuantity = row.SoldQuantity,
            CurrentStock = row.CurrentStock,
            Revenue = row.Revenue
        }).ToList();

        return ApiResponseFactory.Ok("Fast moving inventory retrieved successfully.", response);
    }

    public async Task<ApiResponse<List<PriorityAlertResponse>>> GetPriorityAlertsAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var rows = await _reportQueryRepository.GetPriorityAlertsAsync(limit, cancellationToken);

        var response = rows.Select(row => new PriorityAlertResponse
        {
            Id = row.Id,
            Type = row.Type,
            Severity = row.Severity,
            Title = row.Title,
            Description = row.Description,
            CreatedAt = row.CreatedAt
        }).ToList();

        return ApiResponseFactory.Ok("Priority alerts retrieved successfully.", response);
    }

    private async Task<List<RevenueTrendPointResponse>> BuildDailyTrendAsync(CancellationToken cancellationToken)
    {
        var today = AsUtcDate(DateTime.UtcNow);
        var start = today.AddDays(-6);
        var endExclusive = today.AddDays(1);

        var sales = await _reportQueryRepository.GetCompletedSalesForTrendAsync(start, endExclusive, cancellationToken);

        var points = new List<RevenueTrendPointResponse>(capacity: 7);
        for (var i = 0; i < 7; i++)
        {
            var date = start.AddDays(i);
            var daySales = sales.Where(s => AsUtcDate(s.SaleDate) == date).ToList();

            points.Add(new RevenueTrendPointResponse
            {
                Label = date.ToString("MMM dd", CultureInfo.InvariantCulture),
                Date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Revenue = daySales.Sum(s => s.TotalAmount),
                SalesCount = daySales.Count
            });
        }

        return points;
    }

    private async Task<List<RevenueTrendPointResponse>> BuildWeeklyTrendAsync(CancellationToken cancellationToken)
    {
        var today = AsUtcDate(DateTime.UtcNow);
        var dayOffset = ((int)today.DayOfWeek + 6) % 7;
        var currentWeekStart = today.AddDays(-dayOffset);
        var start = currentWeekStart.AddDays(-49);
        var endExclusive = currentWeekStart.AddDays(7);

        var sales = await _reportQueryRepository.GetCompletedSalesForTrendAsync(start, endExclusive, cancellationToken);

        var points = new List<RevenueTrendPointResponse>(capacity: 8);
        for (var i = 0; i < 8; i++)
        {
            var weekStart = currentWeekStart.AddDays((i - 7) * 7);
            var weekEndExclusive = weekStart.AddDays(7);
            var weeklySales = sales
                .Where(s => s.SaleDate >= weekStart && s.SaleDate < weekEndExclusive)
                .ToList();

            points.Add(new RevenueTrendPointResponse
            {
                Label = $"Wk {ISOWeek.GetWeekOfYear(weekStart)}",
                Date = weekStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Revenue = weeklySales.Sum(s => s.TotalAmount),
                SalesCount = weeklySales.Count
            });
        }

        return points;
    }

    private async Task<List<RevenueTrendPointResponse>> BuildMonthlyTrendAsync(CancellationToken cancellationToken)
    {
        var today = AsUtcDate(DateTime.UtcNow);
        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var start = currentMonthStart.AddMonths(-11);
        var endExclusive = currentMonthStart.AddMonths(1);

        var sales = await _reportQueryRepository.GetCompletedSalesForTrendAsync(start, endExclusive, cancellationToken);

        var points = new List<RevenueTrendPointResponse>(capacity: 12);
        for (var i = 0; i < 12; i++)
        {
            var monthStart = currentMonthStart.AddMonths(i - 11);
            var monthEndExclusive = monthStart.AddMonths(1);
            var monthlySales = sales
                .Where(s => s.SaleDate >= monthStart && s.SaleDate < monthEndExclusive)
                .ToList();

            points.Add(new RevenueTrendPointResponse
            {
                Label = monthStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Date = monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Revenue = monthlySales.Sum(s => s.TotalAmount),
                SalesCount = monthlySales.Count
            });
        }

        return points;
    }

    private static DateTime AsUtcDate(DateTime value) =>
        DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
}
