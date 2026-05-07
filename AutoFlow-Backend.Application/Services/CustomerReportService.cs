using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reports;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Services;

public class CustomerReportService : ICustomerReportService
{
    private readonly IAppDbContext _context;

    public CustomerReportService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<CustomerTopSpenderReportResponse>>> GetTopSpendersAsync(
        CancellationToken cancellationToken = default)
    {
        var topSpenders = await (from sale in _context.Sales.AsNoTracking()
                                 join customer in _context.Customers.AsNoTracking()
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
                                 select new CustomerTopSpenderReportResponse
                                 {
                                     CustomerId = grouped.Key.Id,
                                     FullName = grouped.Key.FullName,
                                     Email = grouped.Key.Email,
                                     Phone = grouped.Key.Phone,
                                     Address = grouped.Key.Address,
                                     PurchaseCount = grouped.Count(),
                                     TotalSpent = grouped.Sum(x => x.sale.TotalAmount),
                                     LastPurchaseDate = grouped.Max(x => x.sale.SaleDate)
                                 })
            .ToListAsync(cancellationToken);

        return ApiResponseFactory.Ok("Top spender report retrieved successfully.", topSpenders);
    }

    public async Task<ApiResponse<List<RegularCustomerReportResponse>>> GetRegularCustomersAsync(
        CancellationToken cancellationToken = default)
    {
        var regularCustomers = await (from sale in _context.Sales.AsNoTracking()
                                      join customer in _context.Customers.AsNoTracking()
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
                                      where grouped.Count() > 3
                                      orderby grouped.Count() descending, grouped.Sum(x => x.sale.TotalAmount) descending
                                      select new RegularCustomerReportResponse
                                      {
                                          CustomerId = grouped.Key.Id,
                                          FullName = grouped.Key.FullName,
                                          Email = grouped.Key.Email,
                                          Phone = grouped.Key.Phone,
                                          Address = grouped.Key.Address,
                                          PurchaseCount = grouped.Count(),
                                          TotalSpent = grouped.Sum(x => x.sale.TotalAmount),
                                          LastPurchaseDate = grouped.Max(x => x.sale.SaleDate)
                                      })
            .ToListAsync(cancellationToken);

        return ApiResponseFactory.Ok("Regular customer report retrieved successfully.", regularCustomers);
    }

    public async Task<ApiResponse<List<PendingCreditCustomerReportResponse>>> GetPendingCreditCustomersAsync(
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddMonths(-1);

        var overdueCreditSales = await (from sale in _context.Sales.AsNoTracking()
                                        join customer in _context.Customers.AsNoTracking()
                                            on sale.CustomerId equals customer.Id
                                        where sale.PaymentMethod == PaymentMethod.Credit
                                              && sale.SaleDate < cutoffDate
                                        orderby sale.SaleDate ascending
                                        select new
                                        {
                                            sale.Id,
                                            sale.CustomerId,
                                            customer.FullName,
                                            customer.Email,
                                            customer.Phone,
                                            customer.Address,
                                            sale.SaleDate,
                                            sale.TotalAmount
                                        })
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var response = overdueCreditSales.Select(x => new PendingCreditCustomerReportResponse
        {
            SaleId = x.Id,
            CustomerId = x.CustomerId,
            FullName = x.FullName,
            Email = x.Email,
            Phone = x.Phone,
            Address = x.Address,
            SaleDate = x.SaleDate,
            CreditAmount = x.TotalAmount,
            DaysOverdue = Math.Max(0, (int)(now - x.SaleDate).TotalDays)
        }).ToList();

        return ApiResponseFactory.Ok("Pending credit customer report retrieved successfully.", response);
    }
}
