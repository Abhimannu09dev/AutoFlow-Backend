using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendInvoiceAsync(Sale sale, CancellationToken cancellationToken = default);
}