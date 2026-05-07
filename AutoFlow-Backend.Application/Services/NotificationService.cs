using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly IPartRepository _partRepository;
    private readonly IAppDbContext _context;

    public NotificationService(
        IEmailService emailService,
        IPartRepository partRepository,
        IAppDbContext context)
    {
        _emailService = emailService;
        _partRepository = partRepository;
        _context = context;
    }

    public async Task<ApiResponse<bool>> SendLowStockAlertAsync(CancellationToken cancellationToken = default)
    {
        var lowStockParts = await _partRepository.GetLowStockActiveAsync(cancellationToken);

        if (lowStockParts.Count == 0)
            return ApiResponseFactory.Ok("No low stock parts found.", true);

        var partLines = lowStockParts.Select(p =>
            $"<li><strong>{p.PartName}</strong> — Stock: {p.StockQuantity} (Minimum: {p.MinimumStockLevel})</li>");

        var body = $@"
            <h2>Low Stock Alert</h2>
            <p>The following parts are below minimum stock level:</p>
            <ul>{string.Join("", partLines)}</ul>
            <p>Please restock as soon as possible.</p>";

        await _emailService.SendAsync(
            to: "admin@autoflow.com",
            subject: "AutoFlow — Low Stock Alert",
            body: body,
            cancellationToken: cancellationToken);

        return ApiResponseFactory.Ok($"Low stock alert sent for {lowStockParts.Count} part(s).", true);
    }

    public async Task<ApiResponse<bool>> SendCreditOverdueRemindersAsync(CancellationToken cancellationToken = default)
    {
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

        var overdueSales = await _context.Sales
            .Include(s => s.Customer)
            .Where(s =>
                s.PaymentMethod == PaymentMethod.Credit &&
                s.SaleDate <= oneMonthAgo)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (overdueSales.Count == 0)
            return ApiResponseFactory.Ok("No overdue credit sales found.", true);

        foreach (var sale in overdueSales)
        {
            if (sale.Customer is null || string.IsNullOrWhiteSpace(sale.Customer.Email))
                continue;

            var body = $@"
                <h2>Payment Reminder</h2>
                <p>Dear {sale.Customer.FullName},</p>
                <p>This is a reminder that your credit payment of <strong>${sale.TotalAmount:F2}</strong>
                from <strong>{sale.SaleDate:MMMM dd, yyyy}</strong> is overdue.</p>
                <p>Please contact us to arrange payment as soon as possible.</p>
                <p>Thank you,<br/>AutoFlow Team</p>";

            await _emailService.SendAsync(
                to: sale.Customer.Email,
                subject: "AutoFlow — Payment Reminder",
                body: body,
                cancellationToken: cancellationToken);
        }

        return ApiResponseFactory.Ok($"Credit overdue reminders sent to {overdueSales.Count} customer(s).", true);
    }
}