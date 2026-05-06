using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reports;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IFinancialReportService
{
    Task<ApiResponse<FinancialReportResponse>> GetDailyReportAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<ApiResponse<FinancialReportResponse>> GetMonthlyReportAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<ApiResponse<FinancialReportResponse>> GetYearlyReportAsync(int year, CancellationToken cancellationToken = default);
}