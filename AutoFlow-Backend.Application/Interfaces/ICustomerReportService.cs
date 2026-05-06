using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reports;

namespace AutoFlow_Backend.Application.Interfaces;

public interface ICustomerReportService
{
    Task<ApiResponse<List<CustomerTopSpenderReportResponse>>> GetTopSpendersAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<RegularCustomerReportResponse>>> GetRegularCustomersAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PendingCreditCustomerReportResponse>>> GetPendingCreditCustomersAsync(CancellationToken cancellationToken = default);
}
