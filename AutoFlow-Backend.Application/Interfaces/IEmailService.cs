using AutoFlow_Backend.Application.DTOs.Sales;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendAdminAlertAsync(string subject, string body, CancellationToken cancellationToken = default);
    Task SendInvoiceAsync(SaleInvoiceDto invoice, CancellationToken cancellationToken = default);
}
