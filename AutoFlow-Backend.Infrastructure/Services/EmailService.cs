using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AutoFlow_Backend.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly InvoiceTemplateBuilder _templateBuilder;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        InvoiceTemplateBuilder templateBuilder)
    {
        _emailSettings = emailSettings.Value;
        _templateBuilder = templateBuilder;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
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