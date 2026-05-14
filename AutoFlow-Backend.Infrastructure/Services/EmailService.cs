using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AutoFlow_Backend.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly InvoiceTemplateBuilder _templateBuilder;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        InvoiceTemplateBuilder templateBuilder,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _templateBuilder = templateBuilder;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls, cancellationToken);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < 2)
            {
                _logger.LogWarning(ex, "Email attempt {Attempt} failed, retrying...", attempt + 1);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
            }
        }
    }

    public async Task SendAdminAlertAsync(
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        await SendAsync(_emailSettings.AdminEmail, subject, body, cancellationToken);
    }

    public async Task SendInvoiceAsync(SaleInvoiceDto invoice, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(invoice.CustomerEmail))
            return;

        var html = _templateBuilder.Build(invoice);
        var subject = "AutoFlow Invoice #" + invoice.InvoiceNumber;
        await SendAsync(invoice.CustomerEmail, subject, html, cancellationToken);
    }
}
