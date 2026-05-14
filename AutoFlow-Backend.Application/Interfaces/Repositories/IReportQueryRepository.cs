using AutoFlow_Backend.Application.Models;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IReportQueryRepository
{
    Task<int> CountSalesAsync(CancellationToken cancellationToken = default);
    Task<decimal> SumSalesRevenueAsync(CancellationToken cancellationToken = default);
    Task<int> CountCompletedSalesAsync(CancellationToken cancellationToken = default);
    Task<decimal> SumCompletedSalesRevenueByDateRangeAsync(
        DateTime startInclusive,
        DateTime endExclusive,
        CancellationToken cancellationToken = default);
    Task<int> CountCustomersAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveStaffAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveVendorsAsync(CancellationToken cancellationToken = default);
    Task<int> CountActivePartsAsync(CancellationToken cancellationToken = default);
    Task<int> CountPurchaseInvoicesAsync(CancellationToken cancellationToken = default);
    Task<int> CountPendingPurchaseInvoicesAsync(CancellationToken cancellationToken = default);
    Task<int> CountAppointmentsAsync(CancellationToken cancellationToken = default);
    Task<int> CountPendingAppointmentsAsync(CancellationToken cancellationToken = default);
    Task<int> CountPendingPartRequestsAsync(CancellationToken cancellationToken = default);
    Task<int> CountReviewsAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetAverageReviewRatingAsync(CancellationToken cancellationToken = default);
    Task<decimal> SumActiveInventoryCostValueAsync(CancellationToken cancellationToken = default);
    Task<List<LowStockPartReadModel>> GetLowStockPartsAsync(CancellationToken cancellationToken = default);
    Task<List<ActivityStreamItemReadModel>> GetDashboardActivityStreamAsync(int limit, CancellationToken cancellationToken = default);
    Task<List<RevenueTrendSaleReadModel>> GetCompletedSalesForTrendAsync(
        DateTime startInclusive,
        DateTime endExclusive,
        CancellationToken cancellationToken = default);
    Task<List<FastMovingInventoryReadModel>> GetFastMovingInventoryAsync(int limit, CancellationToken cancellationToken = default);
    Task<List<PriorityAlertReadModel>> GetPriorityAlertsAsync(int limit, CancellationToken cancellationToken = default);

    Task<List<SaleSummaryReadModel>> GetCompletedSalesByDateRangeAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);

    Task<List<CustomerSummaryResult>> GetTopSpendersAsync(CancellationToken cancellationToken = default);

    Task<List<CustomerSummaryResult>> GetRegularCustomersAsync(int minimumPurchaseCount, CancellationToken cancellationToken = default);

    Task<List<OverdueCreditSaleReadModel>> GetOverdueCreditSalesAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}
