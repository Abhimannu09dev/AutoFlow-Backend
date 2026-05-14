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

    public async Task<int> CountCompletedSalesAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Sales.AsNoTracking()
            .CountAsync(s => s.Status == SaleStatus.Completed, cancellationToken);

    public async Task<decimal> SumCompletedSalesRevenueByDateRangeAsync(
        DateTime startInclusive,
        DateTime endExclusive,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Sales.AsNoTracking()
            .Where(s => s.Status == SaleStatus.Completed && s.SaleDate >= startInclusive && s.SaleDate < endExclusive)
            .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;

    public async Task<int> CountCustomersAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Customers.AsNoTracking().CountAsync(cancellationToken);

    public async Task<int> CountActiveStaffAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Staff.AsNoTracking().CountAsync(s => s.IsActive, cancellationToken);

    public async Task<int> CountActiveVendorsAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Vendors.AsNoTracking().CountAsync(v => v.IsActive, cancellationToken);

    public async Task<int> CountActivePartsAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Parts.AsNoTracking().CountAsync(p => p.IsActive, cancellationToken);

    public async Task<int> CountPurchaseInvoicesAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.PurchaseInvoices.AsNoTracking().CountAsync(cancellationToken);

    public async Task<int> CountPendingPurchaseInvoicesAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.PurchaseInvoices.AsNoTracking().CountAsync(pi => pi.Status == PurchaseInvoiceStatus.Pending, cancellationToken);

    public async Task<int> CountAppointmentsAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Appointments.AsNoTracking().CountAsync(cancellationToken);

    public async Task<int> CountPendingAppointmentsAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Appointments.AsNoTracking().CountAsync(a => a.Status == AppointmentStatus.Pending, cancellationToken);

    public async Task<int> CountPendingPartRequestsAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.PartRequests.AsNoTracking().CountAsync(pr => pr.Status == PartRequestStatus.Pending, cancellationToken);

    public async Task<int> CountReviewsAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Reviews.AsNoTracking().CountAsync(cancellationToken);

    public async Task<decimal> GetAverageReviewRatingAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Reviews.AsNoTracking().AverageAsync(r => (decimal?)r.Rating, cancellationToken) ?? 0m;

    public async Task<decimal> SumActiveInventoryCostValueAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Parts.AsNoTracking()
            .Where(p => p.IsActive)
            .SumAsync(p => (decimal?)(p.UnitPrice * p.StockQuantity), cancellationToken) ?? 0m;

    public async Task<List<LowStockPartReadModel>> GetLowStockPartsAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Parts.AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity < p.MinimumStockLevel)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.PartName)
            .Select(p => new LowStockPartReadModel(p.Id, p.PartName, p.PartNumber, p.StockQuantity, p.MinimumStockLevel))
            .ToListAsync(cancellationToken);

    public async Task<List<ActivityStreamItemReadModel>> GetDashboardActivityStreamAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit, 1, 50);

        var recentSalesRaw = await (from sale in _dbContext.Sales.AsNoTracking()
                                    join staff in _dbContext.Staff.AsNoTracking()
                                        on sale.StaffId equals staff.Id into staffJoin
                                    from staff in staffJoin.DefaultIfEmpty()
                                    orderby sale.CreatedAt descending
                                    select new
                                    {
                                        sale.Id,
                                        sale.InvoiceNumber,
                                        sale.TotalAmount,
                                        sale.CreatedAt,
                                        StaffName = staff != null ? staff.FullName : null
                                    })
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        var recentSales = recentSalesRaw.Select(s => new ActivityStreamItemReadModel(
            Id: s.Id.ToString(),
            Type: "Sale",
            Title: $"Sale {s.InvoiceNumber}",
            Description: $"Sale recorded with total {s.TotalAmount:F2}.",
            CreatedAt: s.CreatedAt,
            ActorName: string.IsNullOrWhiteSpace(s.StaffName) ? "Staff" : s.StaffName)).ToList();

        var recentPurchaseInvoicesRaw = await (from invoice in _dbContext.PurchaseInvoices.AsNoTracking()
                                               join vendor in _dbContext.Vendors.AsNoTracking()
                                                   on invoice.VendorId equals vendor.Id into vendorJoin
                                               from vendor in vendorJoin.DefaultIfEmpty()
                                               orderby invoice.CreatedAt descending
                                               select new
                                               {
                                                   invoice.Id,
                                                   invoice.TotalAmount,
                                                   invoice.CreatedAt,
                                                   VendorName = vendor != null ? vendor.VendorName : null
                                               })
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        var recentPurchaseInvoices = recentPurchaseInvoicesRaw.Select(i => new ActivityStreamItemReadModel(
            Id: i.Id.ToString(),
            Type: "PurchaseInvoice",
            Title: "Purchase invoice created",
            Description: $"Vendor: {(string.IsNullOrWhiteSpace(i.VendorName) ? "Unknown" : i.VendorName)} | Total: {i.TotalAmount:F2}",
            CreatedAt: i.CreatedAt,
            ActorName: string.IsNullOrWhiteSpace(i.VendorName) ? "Vendor" : i.VendorName)).ToList();

        var recentAppointmentsRaw = await (from appointment in _dbContext.Appointments.AsNoTracking()
                                           join customer in _dbContext.Customers.AsNoTracking()
                                               on appointment.CustomerId equals customer.Id into customerJoin
                                           from customer in customerJoin.DefaultIfEmpty()
                                           orderby appointment.CreatedAt descending
                                           select new
                                           {
                                               appointment.Id,
                                               appointment.Date,
                                               appointment.Time,
                                               appointment.Status,
                                               appointment.CreatedAt,
                                               CustomerName = customer != null ? customer.FullName : null
                                           })
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        var recentAppointments = recentAppointmentsRaw.Select(a => new ActivityStreamItemReadModel(
            Id: a.Id.ToString(),
            Type: "Appointment",
            Title: "Appointment created",
            Description: $"{a.Date:yyyy-MM-dd} {a.Time} | Status: {a.Status}",
            CreatedAt: a.CreatedAt,
            ActorName: string.IsNullOrWhiteSpace(a.CustomerName) ? "Customer" : a.CustomerName)).ToList();

        var recentPartRequestsRaw = await (from request in _dbContext.PartRequests.AsNoTracking()
                                           join customer in _dbContext.Customers.AsNoTracking()
                                               on request.CustomerId equals customer.Id into customerJoin
                                           from customer in customerJoin.DefaultIfEmpty()
                                           orderby request.CreatedAt descending
                                           select new
                                           {
                                               request.Id,
                                               request.PartName,
                                               request.Quantity,
                                               request.Status,
                                               request.CreatedAt,
                                               CustomerName = customer != null ? customer.FullName : null
                                           })
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        var recentPartRequests = recentPartRequestsRaw.Select(pr => new ActivityStreamItemReadModel(
            Id: pr.Id.ToString(),
            Type: "PartRequest",
            Title: "Part request submitted",
            Description: $"{pr.PartName} x{pr.Quantity} | Status: {pr.Status}",
            CreatedAt: pr.CreatedAt,
            ActorName: string.IsNullOrWhiteSpace(pr.CustomerName) ? "Customer" : pr.CustomerName)).ToList();

        var recentReviewsRaw = await (from review in _dbContext.Reviews.AsNoTracking()
                                      join customer in _dbContext.Customers.AsNoTracking()
                                          on review.CustomerId equals customer.Id into customerJoin
                                      from customer in customerJoin.DefaultIfEmpty()
                                      orderby review.CreatedAt descending
                                      select new
                                      {
                                          review.Id,
                                          review.Rating,
                                          review.CreatedAt,
                                          CustomerName = customer != null ? customer.FullName : null
                                      })
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        var recentReviews = recentReviewsRaw.Select(r => new ActivityStreamItemReadModel(
            Id: r.Id.ToString(),
            Type: "Review",
            Title: "Customer review submitted",
            Description: $"Rating: {r.Rating}/5",
            CreatedAt: r.CreatedAt,
            ActorName: string.IsNullOrWhiteSpace(r.CustomerName) ? "Customer" : r.CustomerName)).ToList();

        var lowStockActivitiesRaw = await _dbContext.Parts.AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity < p.MinimumStockLevel)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.PartName)
            .Take(normalizedLimit)
            .Select(p => new
            {
                p.Id,
                p.PartName,
                p.PartNumber,
                p.StockQuantity,
                p.MinimumStockLevel,
                CreatedAt = p.UpdatedAt ?? p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var lowStockActivities = lowStockActivitiesRaw.Select(p => new ActivityStreamItemReadModel(
            Id: p.Id.ToString(),
            Type: "LowStock",
            Title: "Low stock alert",
            Description: $"{p.PartName} ({p.PartNumber}) has {p.StockQuantity} in stock (min {p.MinimumStockLevel}).",
            CreatedAt: p.CreatedAt,
            ActorName: "Inventory")).ToList();

        return recentSales
            .Concat(recentPurchaseInvoices)
            .Concat(recentAppointments)
            .Concat(recentPartRequests)
            .Concat(recentReviews)
            .Concat(lowStockActivities)
            .OrderByDescending(item => item.CreatedAt)
            .Take(normalizedLimit)
            .ToList();
    }

    public async Task<List<RevenueTrendSaleReadModel>> GetCompletedSalesForTrendAsync(
        DateTime startInclusive,
        DateTime endExclusive,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Sales.AsNoTracking()
            .Where(s => s.Status == SaleStatus.Completed && s.SaleDate >= startInclusive && s.SaleDate < endExclusive)
            .Select(s => new RevenueTrendSaleReadModel(s.SaleDate, s.TotalAmount))
            .ToListAsync(cancellationToken);

    public async Task<List<FastMovingInventoryReadModel>> GetFastMovingInventoryAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit, 1, 50);

        return await (from saleItem in _dbContext.SaleItems.AsNoTracking()
                      join sale in _dbContext.Sales.AsNoTracking()
                          on saleItem.SaleId equals sale.Id
                      join part in _dbContext.Parts.AsNoTracking()
                          on saleItem.PartId equals part.Id
                      where sale.Status == SaleStatus.Completed
                      group new { saleItem, part } by new
                      {
                          saleItem.PartId,
                          part.PartName,
                          part.PartNumber,
                          part.StockQuantity
                      }
            into grouped
                      orderby grouped.Sum(x => x.saleItem.Quantity) descending
                      select new FastMovingInventoryReadModel(
                          grouped.Key.PartId,
                          grouped.Key.PartName,
                          grouped.Key.PartNumber,
                          grouped.Sum(x => x.saleItem.Quantity),
                          grouped.Key.StockQuantity,
                          grouped.Sum(x => x.saleItem.SubTotal)))
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PriorityAlertReadModel>> GetPriorityAlertsAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit, 1, 50);

        var lowStockAlertsRaw = await _dbContext.Parts.AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity < p.MinimumStockLevel)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.PartName)
            .Take(normalizedLimit)
            .Select(p => new
            {
                p.Id,
                p.PartName,
                p.StockQuantity,
                p.MinimumStockLevel,
                CreatedAt = p.UpdatedAt ?? p.CreatedAt
            })
            .ToListAsync(cancellationToken);
        var lowStockAlerts = lowStockAlertsRaw.Select(p => new PriorityAlertReadModel(
            p.Id.ToString(),
            "LowStock",
            "High",
            $"Low stock: {p.PartName}",
            $"{p.PartName} has {p.StockQuantity} units left (minimum {p.MinimumStockLevel}).",
            p.CreatedAt)).ToList();

        var pendingRequestAlertsRaw = await _dbContext.PartRequests.AsNoTracking()
            .Where(pr => pr.Status == PartRequestStatus.Pending)
            .OrderByDescending(pr => pr.CreatedAt)
            .Take(normalizedLimit)
            .Select(pr => new { pr.Id, pr.PartName, pr.Quantity, pr.CreatedAt })
            .ToListAsync(cancellationToken);
        var pendingRequestAlerts = pendingRequestAlertsRaw.Select(pr => new PriorityAlertReadModel(
            pr.Id.ToString(),
            "PendingRequest",
            "Medium",
            $"Pending part request: {pr.PartName}",
            $"Quantity requested: {pr.Quantity}.",
            pr.CreatedAt)).ToList();

        var pendingAppointmentAlertsRaw = await _dbContext.Appointments.AsNoTracking()
            .Where(a => a.Status == AppointmentStatus.Pending)
            .OrderByDescending(a => a.CreatedAt)
            .Take(normalizedLimit)
            .Select(a => new { a.Id, a.Date, a.Time, a.CreatedAt })
            .ToListAsync(cancellationToken);
        var pendingAppointmentAlerts = pendingAppointmentAlertsRaw.Select(a => new PriorityAlertReadModel(
            a.Id.ToString(),
            "PendingAppointment",
            "Medium",
            "Pending appointment",
            $"Appointment on {a.Date:yyyy-MM-dd} at {a.Time}.",
            a.CreatedAt)).ToList();

        var pendingInvoiceAlertsRaw = await _dbContext.PurchaseInvoices.AsNoTracking()
            .Where(pi => pi.Status == PurchaseInvoiceStatus.Pending)
            .OrderByDescending(pi => pi.CreatedAt)
            .Take(normalizedLimit)
            .Select(pi => new { pi.Id, pi.TotalAmount, pi.CreatedAt })
            .ToListAsync(cancellationToken);
        var pendingInvoiceAlerts = pendingInvoiceAlertsRaw.Select(pi => new PriorityAlertReadModel(
            pi.Id.ToString(),
            "Invoice",
            "Low",
            "Pending purchase invoice",
            $"Purchase invoice total: {pi.TotalAmount:F2}.",
            pi.CreatedAt)).ToList();

        return lowStockAlerts
            .Concat(pendingRequestAlerts)
            .Concat(pendingAppointmentAlerts)
            .Concat(pendingInvoiceAlerts)
            .OrderByDescending(item => item.CreatedAt)
            .Take(normalizedLimit)
            .ToList();
    }

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
        var results = await (from sale in _dbContext.Sales.AsNoTracking()
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
                      select new
                      {
                          grouped.Key.Id,
                          grouped.Key.FullName,
                          grouped.Key.Email,
                          grouped.Key.Phone,
                          grouped.Key.Address,
                          PurchaseCount = grouped.Count(),
                          TotalSpent = grouped.Sum(x => x.sale.TotalAmount),
                          LastPurchaseDate = grouped.Max(x => x.sale.SaleDate)
                      })
            .ToListAsync(cancellationToken);

        return results.Select(r => new CustomerSummaryResult(
            r.Id,
            r.FullName,
            r.Email,
            r.Phone,
            r.Address,
            r.PurchaseCount,
            r.TotalSpent,
            r.LastPurchaseDate))
            .ToList();
    }

    public async Task<List<CustomerSummaryResult>>
        GetRegularCustomersAsync(int minimumPurchaseCount, CancellationToken cancellationToken = default)
    {
        var results = await (from sale in _dbContext.Sales.AsNoTracking()
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
                      select new
                      {
                          grouped.Key.Id,
                          grouped.Key.FullName,
                          grouped.Key.Email,
                          grouped.Key.Phone,
                          grouped.Key.Address,
                          PurchaseCount = grouped.Count(),
                          TotalSpent = grouped.Sum(x => x.sale.TotalAmount),
                          LastPurchaseDate = grouped.Max(x => x.sale.SaleDate)
                      })
            .ToListAsync(cancellationToken);

        return results.Select(r => new CustomerSummaryResult(
            r.Id,
            r.FullName,
            r.Email,
            r.Phone,
            r.Address,
            r.PurchaseCount,
            r.TotalSpent,
            r.LastPurchaseDate))
            .ToList();
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
