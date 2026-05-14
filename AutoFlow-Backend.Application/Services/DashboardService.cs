using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Dashboard;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AutoFlow_Backend.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IReportQueryRepository _reportQueryRepository;
    private readonly DbContext _dbContext;

    public DashboardService(
        IReportQueryRepository reportQueryRepository,
        DbContext dbContext)
    {
        _reportQueryRepository = reportQueryRepository;
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<DashboardResponse>> GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var utcToday = utcNow.Date;
        var monthStart = new DateTime(utcToday.Year, utcToday.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);
        var yearStart = new DateTime(utcToday.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = yearStart.AddYears(1);

        var totalSalesCount = await _reportQueryRepository.CountSalesAsync(cancellationToken);
        var totalRevenue = await _reportQueryRepository.SumSalesRevenueAsync(cancellationToken);
        var totalCustomersCount = await _reportQueryRepository.CountCustomersAsync(cancellationToken);
        var totalStaffCount = await _reportQueryRepository.CountActiveStaffAsync(cancellationToken);
        var lowStockParts = await _reportQueryRepository.GetLowStockPartsAsync(cancellationToken);

        var completedSalesCount = await _dbContext.Set<Sale>()
            .AsNoTracking()
            .CountAsync(s => s.Status == SaleStatus.Completed, cancellationToken);

        var todayRevenue = await _dbContext.Set<Sale>()
            .AsNoTracking()
            .Where(s => s.Status == SaleStatus.Completed && s.SaleDate >= utcToday && s.SaleDate < utcToday.AddDays(1))
            .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;

        var monthlyRevenue = await _dbContext.Set<Sale>()
            .AsNoTracking()
            .Where(s => s.Status == SaleStatus.Completed && s.SaleDate >= monthStart && s.SaleDate < monthEnd)
            .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;

        var yearlyRevenue = await _dbContext.Set<Sale>()
            .AsNoTracking()
            .Where(s => s.Status == SaleStatus.Completed && s.SaleDate >= yearStart && s.SaleDate < yearEnd)
            .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;

        var totalVendorsCount = await _dbContext.Set<Vendor>()
            .AsNoTracking()
            .CountAsync(v => v.IsActive, cancellationToken);

        var totalPartsCount = await _dbContext.Set<Part>()
            .AsNoTracking()
            .CountAsync(p => p.IsActive, cancellationToken);

        var totalPurchaseInvoicesCount = await _dbContext.Set<PurchaseInvoice>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var pendingInvoicesCount = await _dbContext.Set<PurchaseInvoice>()
            .AsNoTracking()
            .CountAsync(pi => pi.Status == PurchaseInvoiceStatus.Pending, cancellationToken);

        var totalAppointmentsCount = await _dbContext.Set<Appointment>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var pendingAppointmentsCount = await _dbContext.Set<Appointment>()
            .AsNoTracking()
            .CountAsync(a => a.Status == AppointmentStatus.Pending, cancellationToken);

        var pendingPartRequestsCount = await _dbContext.Set<PartRequest>()
            .AsNoTracking()
            .CountAsync(pr => pr.Status == PartRequestStatus.Pending, cancellationToken);

        var totalReviewsCount = await _dbContext.Set<Review>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var averageReviewRating = await _dbContext.Set<Review>()
            .AsNoTracking()
            .AverageAsync(r => (decimal?)r.Rating, cancellationToken) ?? 0m;

        var totalInventoryValue = await _dbContext.Set<Part>()
            .AsNoTracking()
            .Where(p => p.IsActive)
            .SumAsync(p => (decimal?)(p.UnitPrice * p.StockQuantity), cancellationToken) ?? 0m;

        var dashboard = new DashboardResponse
        {
            TotalSalesCount = totalSalesCount,
            CompletedSalesCount = completedSalesCount,
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue,
            MonthlyRevenue = monthlyRevenue,
            YearlyRevenue = yearlyRevenue,
            TotalCustomersCount = totalCustomersCount,
            TotalStaffCount = totalStaffCount,
            TotalVendorsCount = totalVendorsCount,
            TotalPartsCount = totalPartsCount,
            TotalPurchaseInvoicesCount = totalPurchaseInvoicesCount,
            PendingInvoicesCount = pendingInvoicesCount,
            TotalAppointmentsCount = totalAppointmentsCount,
            PendingAppointmentsCount = pendingAppointmentsCount,
            PendingPartRequestsCount = pendingPartRequestsCount,
            TotalReviewsCount = totalReviewsCount,
            AverageReviewRating = Math.Round(averageReviewRating, 2, MidpointRounding.AwayFromZero),
            LowStockPartsCount = lowStockParts.Count,
            TotalInventoryValue = totalInventoryValue,
            LowStockParts = lowStockParts.Select(part => new LowStockPartDashboardResponse
            {
                PartId = part.Id,
                PartName = part.PartName,
                PartNumber = part.PartNumber,
                StockQuantity = part.StockQuantity,
                MinimumStockLevel = part.MinimumStockLevel
            }).ToList()
        };

        return ApiResponseFactory.Ok("Dashboard data retrieved successfully.", dashboard);
    }

    public async Task<ApiResponse<List<ActivityStreamItemResponse>>> GetActivityStreamAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit, 1, 50);

        var recentSales = await (from sale in _dbContext.Set<Sale>().AsNoTracking()
                                 join staff in _dbContext.Set<Staff>().AsNoTracking()
                                     on sale.StaffId equals staff.Id into staffJoin
                                 from staff in staffJoin.DefaultIfEmpty()
                                 orderby sale.CreatedAt descending
                                 select new ActivityStreamItemResponse
                                 {
                                     Id = sale.Id.ToString(),
                                     Type = "Sale",
                                     Title = $"Sale {sale.InvoiceNumber}",
                                     Description = $"Sale recorded with total {sale.TotalAmount:F2}.",
                                     CreatedAt = sale.CreatedAt,
                                     ActorName = staff != null ? staff.FullName : "Staff"
                                 })
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        var recentPurchaseInvoices = await (from invoice in _dbContext.Set<PurchaseInvoice>().AsNoTracking()
                                            join vendor in _dbContext.Set<Vendor>().AsNoTracking()
                                                on invoice.VendorId equals vendor.Id into vendorJoin
                                            from vendor in vendorJoin.DefaultIfEmpty()
                                            orderby invoice.CreatedAt descending
                                            select new ActivityStreamItemResponse
                                            {
                                                Id = invoice.Id.ToString(),
                                                Type = "PurchaseInvoice",
                                                Title = "Purchase invoice created",
                                                Description = $"Vendor: {(vendor != null ? vendor.VendorName : "Unknown")} | Total: {invoice.TotalAmount:F2}",
                                                CreatedAt = invoice.CreatedAt,
                                                ActorName = vendor != null ? vendor.VendorName : "Vendor"
                                            })
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        var recentAppointments = await (from appointment in _dbContext.Set<Appointment>().AsNoTracking()
                                        join customer in _dbContext.Set<Customer>().AsNoTracking()
                                            on appointment.CustomerId equals customer.Id into customerJoin
                                        from customer in customerJoin.DefaultIfEmpty()
                                        orderby appointment.CreatedAt descending
                                        select new ActivityStreamItemResponse
                                        {
                                            Id = appointment.Id.ToString(),
                                            Type = "Appointment",
                                            Title = "Appointment created",
                                            Description = $"{appointment.Date:yyyy-MM-dd} {appointment.Time} | Status: {appointment.Status}",
                                            CreatedAt = appointment.CreatedAt,
                                            ActorName = customer != null ? customer.FullName : "Customer"
                                        })
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        var recentPartRequests = await (from request in _dbContext.Set<PartRequest>().AsNoTracking()
                                        join customer in _dbContext.Set<Customer>().AsNoTracking()
                                            on request.CustomerId equals customer.Id into customerJoin
                                        from customer in customerJoin.DefaultIfEmpty()
                                        orderby request.CreatedAt descending
                                        select new ActivityStreamItemResponse
                                        {
                                            Id = request.Id.ToString(),
                                            Type = "PartRequest",
                                            Title = "Part request submitted",
                                            Description = $"{request.PartName} x{request.Quantity} | Status: {request.Status}",
                                            CreatedAt = request.CreatedAt,
                                            ActorName = customer != null ? customer.FullName : "Customer"
                                        })
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        var recentReviews = await (from review in _dbContext.Set<Review>().AsNoTracking()
                                   join customer in _dbContext.Set<Customer>().AsNoTracking()
                                       on review.CustomerId equals customer.Id into customerJoin
                                   from customer in customerJoin.DefaultIfEmpty()
                                   orderby review.CreatedAt descending
                                   select new ActivityStreamItemResponse
                                   {
                                       Id = review.Id.ToString(),
                                       Type = "Review",
                                       Title = "Customer review submitted",
                                       Description = $"Rating: {review.Rating}/5",
                                       CreatedAt = review.CreatedAt,
                                       ActorName = customer != null ? customer.FullName : "Customer"
                                   })
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        var lowStockActivities = await _dbContext.Set<Part>()
            .AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity < p.MinimumStockLevel)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.PartName)
            .Take(normalizedLimit)
            .Select(p => new ActivityStreamItemResponse
            {
                Id = p.Id.ToString(),
                Type = "LowStock",
                Title = "Low stock alert",
                Description = $"{p.PartName} ({p.PartNumber}) has {p.StockQuantity} in stock (min {p.MinimumStockLevel}).",
                CreatedAt = p.UpdatedAt ?? p.CreatedAt,
                ActorName = "Inventory"
            })
            .ToListAsync(cancellationToken);

        var merged = recentSales
            .Concat(recentPurchaseInvoices)
            .Concat(recentAppointments)
            .Concat(recentPartRequests)
            .Concat(recentReviews)
            .Concat(lowStockActivities)
            .OrderByDescending(item => item.CreatedAt)
            .Take(normalizedLimit)
            .ToList();

        return ApiResponseFactory.Ok("Dashboard activity stream retrieved successfully.", merged);
    }

    public async Task<ApiResponse<List<RevenueTrendPointResponse>>> GetRevenueTrendAsync(
        string range,
        CancellationToken cancellationToken = default)
    {
        var normalizedRange = string.IsNullOrWhiteSpace(range) ? "daily" : range.Trim().ToLowerInvariant();

        var points = normalizedRange switch
        {
            "daily" => await BuildDailyTrendAsync(cancellationToken),
            "weekly" => await BuildWeeklyTrendAsync(cancellationToken),
            "monthly" => await BuildMonthlyTrendAsync(cancellationToken),
            _ => null
        };

        if (points is null)
            return ApiResponseFactory.Fail<List<RevenueTrendPointResponse>>("Range must be daily, weekly, or monthly.");

        return ApiResponseFactory.Ok("Dashboard revenue trend retrieved successfully.", points);
    }

    public async Task<ApiResponse<List<FastMovingInventoryResponse>>> GetFastMovingInventoryAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit, 1, 50);

        var rows = await (from saleItem in _dbContext.Set<SaleItem>().AsNoTracking()
                          join sale in _dbContext.Set<Sale>().AsNoTracking()
                              on saleItem.SaleId equals sale.Id
                          join part in _dbContext.Set<Part>().AsNoTracking()
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
                          select new FastMovingInventoryResponse
                          {
                              PartId = grouped.Key.PartId,
                              PartName = grouped.Key.PartName,
                              PartNumber = grouped.Key.PartNumber,
                              SoldQuantity = grouped.Sum(x => x.saleItem.Quantity),
                              CurrentStock = grouped.Key.StockQuantity,
                              Revenue = grouped.Sum(x => x.saleItem.SubTotal)
                          })
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        return ApiResponseFactory.Ok("Fast moving inventory retrieved successfully.", rows);
    }

    public async Task<ApiResponse<List<PriorityAlertResponse>>> GetPriorityAlertsAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit, 1, 50);

        var lowStockAlerts = await _dbContext.Set<Part>()
            .AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity < p.MinimumStockLevel)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.PartName)
            .Take(normalizedLimit)
            .Select(p => new PriorityAlertResponse
            {
                Id = p.Id.ToString(),
                Type = "LowStock",
                Severity = "High",
                Title = $"Low stock: {p.PartName}",
                Description = $"{p.PartName} has {p.StockQuantity} units left (minimum {p.MinimumStockLevel}).",
                CreatedAt = p.UpdatedAt ?? p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var pendingRequestAlerts = await _dbContext.Set<PartRequest>()
            .AsNoTracking()
            .Where(pr => pr.Status == PartRequestStatus.Pending)
            .OrderByDescending(pr => pr.CreatedAt)
            .Take(normalizedLimit)
            .Select(pr => new PriorityAlertResponse
            {
                Id = pr.Id.ToString(),
                Type = "PendingRequest",
                Severity = "Medium",
                Title = $"Pending part request: {pr.PartName}",
                Description = $"Quantity requested: {pr.Quantity}.",
                CreatedAt = pr.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var pendingAppointmentAlerts = await _dbContext.Set<Appointment>()
            .AsNoTracking()
            .Where(a => a.Status == AppointmentStatus.Pending)
            .OrderByDescending(a => a.CreatedAt)
            .Take(normalizedLimit)
            .Select(a => new PriorityAlertResponse
            {
                Id = a.Id.ToString(),
                Type = "PendingAppointment",
                Severity = "Medium",
                Title = "Pending appointment",
                Description = $"Appointment on {a.Date:yyyy-MM-dd} at {a.Time}.",
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var pendingInvoiceAlerts = await _dbContext.Set<PurchaseInvoice>()
            .AsNoTracking()
            .Where(pi => pi.Status == PurchaseInvoiceStatus.Pending)
            .OrderByDescending(pi => pi.CreatedAt)
            .Take(normalizedLimit)
            .Select(pi => new PriorityAlertResponse
            {
                Id = pi.Id.ToString(),
                Type = "Invoice",
                Severity = "Low",
                Title = "Pending purchase invoice",
                Description = $"Purchase invoice total: {pi.TotalAmount:F2}.",
                CreatedAt = pi.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var merged = lowStockAlerts
            .Concat(pendingRequestAlerts)
            .Concat(pendingAppointmentAlerts)
            .Concat(pendingInvoiceAlerts)
            .OrderByDescending(a => a.CreatedAt)
            .Take(normalizedLimit)
            .ToList();

        return ApiResponseFactory.Ok("Priority alerts retrieved successfully.", merged);
    }

    private async Task<List<RevenueTrendPointResponse>> BuildDailyTrendAsync(CancellationToken cancellationToken)
    {
        var today = AsUtcDate(DateTime.UtcNow);
        var start = today.AddDays(-6);
        var endExclusive = today.AddDays(1);

        var sales = await _dbContext.Set<Sale>()
            .AsNoTracking()
            .Where(s => s.Status == SaleStatus.Completed && s.SaleDate >= start && s.SaleDate < endExclusive)
            .Select(s => new { s.SaleDate, s.TotalAmount })
            .ToListAsync(cancellationToken);

        var points = new List<RevenueTrendPointResponse>(capacity: 7);
        for (var i = 0; i < 7; i++)
        {
            var date = start.AddDays(i);
            var daySales = sales.Where(s => s.SaleDate.Date == date).ToList();

            points.Add(new RevenueTrendPointResponse
            {
                Label = date.ToString("MMM dd", CultureInfo.InvariantCulture),
                Date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Revenue = daySales.Sum(s => s.TotalAmount),
                SalesCount = daySales.Count
            });
        }

        return points;
    }

    private async Task<List<RevenueTrendPointResponse>> BuildWeeklyTrendAsync(CancellationToken cancellationToken)
    {
        var today = AsUtcDate(DateTime.UtcNow);
        var dayOffset = ((int)today.DayOfWeek + 6) % 7;
        var currentWeekStart = today.AddDays(-dayOffset);
        var start = currentWeekStart.AddDays(-49);
        var endExclusive = currentWeekStart.AddDays(7);

        var sales = await _dbContext.Set<Sale>()
            .AsNoTracking()
            .Where(s => s.Status == SaleStatus.Completed && s.SaleDate >= start && s.SaleDate < endExclusive)
            .Select(s => new { s.SaleDate, s.TotalAmount })
            .ToListAsync(cancellationToken);

        var points = new List<RevenueTrendPointResponse>(capacity: 8);
        for (var i = 0; i < 8; i++)
        {
            var weekStart = currentWeekStart.AddDays((i - 7) * 7);
            var weekEndExclusive = weekStart.AddDays(7);
            var weeklySales = sales
                .Where(s => s.SaleDate >= weekStart && s.SaleDate < weekEndExclusive)
                .ToList();

            points.Add(new RevenueTrendPointResponse
            {
                Label = $"Wk {ISOWeek.GetWeekOfYear(weekStart)}",
                Date = weekStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Revenue = weeklySales.Sum(s => s.TotalAmount),
                SalesCount = weeklySales.Count
            });
        }

        return points;
    }

    private async Task<List<RevenueTrendPointResponse>> BuildMonthlyTrendAsync(CancellationToken cancellationToken)
    {
        var today = AsUtcDate(DateTime.UtcNow);
        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var start = currentMonthStart.AddMonths(-11);
        var endExclusive = currentMonthStart.AddMonths(1);

        var sales = await _dbContext.Set<Sale>()
            .AsNoTracking()
            .Where(s => s.Status == SaleStatus.Completed && s.SaleDate >= start && s.SaleDate < endExclusive)
            .Select(s => new { s.SaleDate, s.TotalAmount })
            .ToListAsync(cancellationToken);

        var points = new List<RevenueTrendPointResponse>(capacity: 12);
        for (var i = 0; i < 12; i++)
        {
            var monthStart = currentMonthStart.AddMonths(i - 11);
            var monthEndExclusive = monthStart.AddMonths(1);
            var monthlySales = sales
                .Where(s => s.SaleDate >= monthStart && s.SaleDate < monthEndExclusive)
                .ToList();

            points.Add(new RevenueTrendPointResponse
            {
                Label = monthStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Date = monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Revenue = monthlySales.Sum(s => s.TotalAmount),
                SalesCount = monthlySales.Count
            });
        }

        return points;
    }

    private static DateTime AsUtcDate(DateTime value) =>
        DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
}
