using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IReportQueryRepository
{
    Task<int> CountSalesAsync(CancellationToken cancellationToken = default);
    Task<decimal> SumSalesRevenueAsync(CancellationToken cancellationToken = default);
    Task<int> CountCustomersAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveStaffAsync(CancellationToken cancellationToken = default);
    Task<List<Part>> GetLowStockPartsAsync(CancellationToken cancellationToken = default);

    Task<List<Sale>> GetCompletedSalesByDateRangeAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);

    Task<List<(Guid CustomerId, string FullName, string Email, string? Phone, string? Address, int PurchaseCount, decimal TotalSpent, DateTime LastPurchaseDate)>>
        GetTopSpendersAsync(CancellationToken cancellationToken = default);

    Task<List<(Guid CustomerId, string FullName, string Email, string? Phone, string? Address, int PurchaseCount, decimal TotalSpent, DateTime LastPurchaseDate)>>
        GetRegularCustomersAsync(int minimumPurchaseCount, CancellationToken cancellationToken = default);

    Task<List<Sale>> GetOverdueCreditSalesAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}