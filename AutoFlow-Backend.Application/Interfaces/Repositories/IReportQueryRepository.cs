using AutoFlow_Backend.Application.Models;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IReportQueryRepository
{
    Task<int> CountSalesAsync(CancellationToken cancellationToken = default);
    Task<decimal> SumSalesRevenueAsync(CancellationToken cancellationToken = default);
    Task<int> CountCustomersAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveStaffAsync(CancellationToken cancellationToken = default);
    Task<List<LowStockPartReadModel>> GetLowStockPartsAsync(CancellationToken cancellationToken = default);

    Task<List<SaleSummaryReadModel>> GetCompletedSalesByDateRangeAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);

    Task<List<CustomerSummaryResult>> GetTopSpendersAsync(CancellationToken cancellationToken = default);

    Task<List<CustomerSummaryResult>> GetRegularCustomersAsync(int minimumPurchaseCount, CancellationToken cancellationToken = default);

    Task<List<OverdueCreditSaleReadModel>> GetOverdueCreditSalesAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}