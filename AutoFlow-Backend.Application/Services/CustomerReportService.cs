using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Reports;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using Microsoft.Extensions.Options;

namespace AutoFlow_Backend.Application.Services;

public class CustomerReportService : ICustomerReportService
{
    private readonly BusinessRulesSettings _businessRules;

    private readonly IReportQueryRepository _reportQueryRepository;

    public CustomerReportService(
        IReportQueryRepository reportQueryRepository,
        IOptions<BusinessRulesSettings> businessRules)
    {
        _reportQueryRepository = reportQueryRepository;
        _businessRules = businessRules.Value;
    }

    public async Task<ApiResponse<List<CustomerTopSpenderReportResponse>>> GetTopSpendersAsync(
        CancellationToken cancellationToken = default)
    {
        var spenders = await _reportQueryRepository.GetTopSpendersAsync(cancellationToken);

        var response = spenders.Select(x => new CustomerTopSpenderReportResponse
        {
            CustomerId = x.CustomerId,
            FullName = x.FullName,
            Email = x.Email,
            Phone = x.Phone,
            Address = x.Address,
            PurchaseCount = x.PurchaseCount,
            TotalSpent = x.TotalSpent,
            LastPurchaseDate = x.LastPurchaseDate
        }).ToList();

        return ApiResponseFactory.Ok("Top spender report retrieved successfully.", response);
    }

    public async Task<ApiResponse<List<RegularCustomerReportResponse>>> GetRegularCustomersAsync(
        CancellationToken cancellationToken = default)
    {
        var regulars = await _reportQueryRepository.GetRegularCustomersAsync(
            _businessRules.RegularCustomerMinimumPurchases, cancellationToken);

        var response = regulars.Select(x => new RegularCustomerReportResponse
        {
            CustomerId = x.CustomerId,
            FullName = x.FullName,
            Email = x.Email,
            Phone = x.Phone,
            Address = x.Address,
            PurchaseCount = x.PurchaseCount,
            TotalSpent = x.TotalSpent,
            LastPurchaseDate = x.LastPurchaseDate
        }).ToList();

        return ApiResponseFactory.Ok("Regular customer report retrieved successfully.", response);
    }

    public async Task<ApiResponse<List<PendingCreditCustomerReportResponse>>> GetPendingCreditCustomersAsync(
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddMonths(-1);
        var overdueSales = await _reportQueryRepository.GetOverdueCreditSalesAsync(cutoffDate, cancellationToken);

        var now = DateTime.UtcNow;
        var response = overdueSales.Select(sale => new PendingCreditCustomerReportResponse
        {
            SaleId = sale.SaleId,
            CustomerId = sale.CustomerId,
            FullName = sale.CustomerName,
            Email = sale.CustomerEmail,
            Phone = sale.Phone,
            Address = sale.Address,
            SaleDate = sale.SaleDate,
            CreditAmount = sale.TotalAmount,
            DaysOverdue = Math.Max(0, (int)(now - sale.SaleDate).TotalDays)
        }).ToList();

        return ApiResponseFactory.Ok("Pending credit customer report retrieved successfully.", response);
    }
}