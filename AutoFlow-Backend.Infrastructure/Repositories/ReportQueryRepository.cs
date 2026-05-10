using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Application.Models;
using AutoFlow_Backend.Domain.Enums;
using AutoFlow_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Repositories;

public class ReportQueryRepository : IReportQueryRepository
{
    private readonly AppDbContext _dbContext;

    public ReportQueryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CountSalesAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Sales.AsNoTracking().CountAsync(cancellationToken);

    public async Task<decimal> SumSalesRevenueAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Sales.AsNoTracking()
            .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;

    public async Task<int> CountCustomersAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Customers.AsNoTracking().CountAsync(cancellationToken);

    public async Task<int> CountActiveStaffAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Staff.AsNoTracking().CountAsync(s => s.IsActive, cancellationToken);

    public async Task<List<LowStockPartReadModel>> GetLowStockPartsAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Parts.AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity < p.MinimumStockLevel)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.PartName)
            .Select(p => new LowStockPartReadModel(p.Id, p.PartName, p.PartNumber, p.StockQuantity, p.MinimumStockLevel))
            .ToListAsync(cancellationToken);

    public async Task<List<SaleSummaryReadModel>> GetCompletedSalesByDateRangeAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Sales.AsNoTracking()
            .Where(s => s.SaleDate >= start && s.SaleDate <= end && s.Status == SaleStatus.Completed)
            .Select(s => new SaleSummaryReadModel(s.SubTotal, s.DiscountAmount, s.TotalAmount))
            .ToListAsync(cancellationToken);

    public async Task<List<CustomerSummaryResult>>
        GetTopSpendersAsync(CancellationToken cancellationToken = default)
    {
        return await (from sale in _dbContext.Sales.AsNoTracking()
                      join customer in _dbContext.Customers.AsNoTracking()
                          on sale.CustomerId equals customer.Id
                      group new { sale, customer } by new
                      {
                          customer.Id,
                          customer.FullName,
                          customer.Email,
                          customer.Phone,
                          customer.Address
                      }
                      into grouped
                      orderby grouped.Sum(x => x.sale.TotalAmount) descending
                      select new CustomerSummaryResult(
                          grouped.Key.Id,
                          grouped.Key.FullName,
                          grouped.Key.Email,
                          grouped.Key.Phone,
                          grouped.Key.Address,
                          grouped.Count(),
                          grouped.Sum(x => x.sale.TotalAmount),
                          grouped.Max(x => x.sale.SaleDate)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CustomerSummaryResult>>
        GetRegularCustomersAsync(int minimumPurchaseCount, CancellationToken cancellationToken = default)
    {
        return await (from sale in _dbContext.Sales.AsNoTracking()
                      join customer in _dbContext.Customers.AsNoTracking()
                          on sale.CustomerId equals customer.Id
                      group new { sale, customer } by new
                      {
                          customer.Id,
                          customer.FullName,
                          customer.Email,
                          customer.Phone,
                          customer.Address
                      }
                      into grouped
                      where grouped.Count() > minimumPurchaseCount
                      orderby grouped.Count() descending, grouped.Sum(x => x.sale.TotalAmount) descending
                      select new CustomerSummaryResult(
                          grouped.Key.Id,
                          grouped.Key.FullName,
                          grouped.Key.Email,
                          grouped.Key.Phone,
                          grouped.Key.Address,
                          grouped.Count(),
                          grouped.Sum(x => x.sale.TotalAmount),
                          grouped.Max(x => x.sale.SaleDate)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OverdueCreditSaleReadModel>> GetOverdueCreditSalesAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Sales
            .Where(s => s.PaymentMethod == PaymentMethod.Credit && s.SaleDate <= cutoffDate)
            .Join(_dbContext.Customers.AsNoTracking(),
                sale => sale.CustomerId,
                customer => customer.Id,
                (sale, customer) => new OverdueCreditSaleReadModel(
                    sale.Id,
                    sale.CustomerId,
                    sale.SaleDate,
                    sale.TotalAmount,
                    customer.FullName,
                    customer.Email,
                    customer.Phone,
                    customer.Address))
            .ToListAsync(cancellationToken);
}