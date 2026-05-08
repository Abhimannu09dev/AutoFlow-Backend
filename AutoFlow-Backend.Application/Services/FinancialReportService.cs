using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reports;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;

namespace AutoFlow_Backend.Application.Services;

public class FinancialReportService : IFinancialReportService
{
    private readonly IReportQueryRepository _reportQueryRepository;

    public FinancialReportService(IReportQueryRepository reportQueryRepository)
    {
        _reportQueryRepository = reportQueryRepository;
    }

    public async Task<ApiResponse<FinancialReportResponse>> GetDailyReportAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        var sales = await _reportQueryRepository.GetCompletedSalesByDateRangeAsync(start, end, cancellationToken);

        var report = new FinancialReportResponse
        {
            Period = date.ToString("MMMM dd, yyyy"),
            TotalSales = sales.Count,
            TotalRevenue = sales.Sum(s => s.SubTotal),
            TotalDiscount = sales.Sum(s => s.DiscountAmount),
            NetRevenue = sales.Sum(s => s.TotalAmount)
        };

        return ApiResponseFactory.Ok("Daily financial report retrieved successfully.", report);
    }

    public async Task<ApiResponse<FinancialReportResponse>> GetMonthlyReportAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        if (month < 1 || month > 12)
            return ApiResponseFactory.Fail<FinancialReportResponse>("Month must be between 1 and 12.");

        if (year < 2000 || year > DateTime.UtcNow.Year)
            return ApiResponseFactory.Fail<FinancialReportResponse>("Invalid year.");

        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddTicks(-1);
        var sales = await _reportQueryRepository.GetCompletedSalesByDateRangeAsync(start, end, cancellationToken);

        var report = new FinancialReportResponse
        {
            Period = start.ToString("MMMM yyyy"),
            TotalSales = sales.Count,
            TotalRevenue = sales.Sum(s => s.SubTotal),
            TotalDiscount = sales.Sum(s => s.DiscountAmount),
            NetRevenue = sales.Sum(s => s.TotalAmount)
        };

        return ApiResponseFactory.Ok("Monthly financial report retrieved successfully.", report);
    }

    public async Task<ApiResponse<FinancialReportResponse>> GetYearlyReportAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        if (year < 2000 || year > DateTime.UtcNow.Year)
            return ApiResponseFactory.Fail<FinancialReportResponse>("Invalid year.");

        var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var sales = await _reportQueryRepository.GetCompletedSalesByDateRangeAsync(start, end, cancellationToken);

        var report = new FinancialReportResponse
        {
            Period = year.ToString(),
            TotalSales = sales.Count,
            TotalRevenue = sales.Sum(s => s.SubTotal),
            TotalDiscount = sales.Sum(s => s.DiscountAmount),
            NetRevenue = sales.Sum(s => s.TotalAmount)
        };

        return ApiResponseFactory.Ok("Yearly financial report retrieved successfully.", report);
    }
}