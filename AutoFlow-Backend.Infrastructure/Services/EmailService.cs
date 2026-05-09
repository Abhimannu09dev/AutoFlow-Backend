using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AutoFlow_Backend.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
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

    public async Task SendInvoiceAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        if (sale.Customer is null || string.IsNullOrWhiteSpace(sale.Customer.Email))
            return;

        var html = BuildInvoiceHtml(sale);
        var subject = $"AutoFlow Invoice #{sale.InvoiceNumber}";
        await SendAsync(sale.Customer.Email, subject, html, cancellationToken);
    }

    private static string BuildInvoiceHtml(Sale sale)
    {
        var itemRows = string.Join("", sale.SaleItems.Select(si =>
            "<tr>" +
            "<td style=\"padding:8px;border:1px solid #ddd;\">" + (si.Part != null ? si.Part.PartName : "N/A") + "</td>" +
            "<td style=\"padding:8px;border:1px solid #ddd;text-align:center;\">" + si.Quantity + "</td>" +
            "<td style=\"padding:8px;border:1px solid #ddd;text-align:right;\">$" + si.UnitPrice.ToString("F2") + "</td>" +
            "<td style=\"padding:8px;border:1px solid #ddd;text-align:right;\">$" + si.SubTotal.ToString("F2") + "</td>" +
            "</tr>"));

        var phoneLine = string.IsNullOrWhiteSpace(sale.Customer?.Phone)
            ? ""
            : "<p style=\"margin:2px 0;color:#555;font-size:13px;\">" + sale.Customer.Phone + "</p>";
        var staffLine = sale.Staff is null
            ? ""
            : "<p style=\"margin:2px 0;color:#555;font-size:13px;\">Issued by: " + sale.Staff.FullName + "</p>";
        var discountLine = sale.DiscountAmount > 0
            ? "<p style=\"color:#28a745;\">Discount: -$" + sale.DiscountAmount.ToString("F2") + "</p>"
            : "";

        return "<!DOCTYPE html>\n" +
            "<html><head><meta charset=\"utf-8\"/><style>\n" +
            "body{font-family:Arial,sans-serif;margin:0;padding:20px;background:#f5f5f5;}\n" +
            ".container{max-width:700px;margin:0 auto;background:#fff;padding:30px;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,0.1);}\n" +
            ".header{background:#1a3c6e;color:#fff;padding:25px;border-radius:8px 8px 0 0;text-align:center;}\n" +
            ".header h1{margin:0;font-size:28px;}\n" +
            ".header p{margin:5px 0 0;color:#cce;font-size:14px;}\n" +
            ".invoice-meta{margin:20px 0;padding:15px;background:#f8f9fa;border-radius:4px;}\n" +
            ".invoice-meta table{width:100%;}\n" +
            ".invoice-meta td{vertical-align:top;}\n" +
            ".meta-right{text-align:right;}\n" +
            ".invoice-meta h2{margin:0 0 5px;color:#1a3c6e;font-size:22px;}\n" +
            ".items-table{width:100%;border-collapse:collapse;margin:20px 0;}\n" +
            ".items-table th{background:#1a3c6e;color:#fff;padding:10px 8px;text-align:left;}\n" +
            ".totals{margin:15px 0;text-align:right;}\n" +
            ".totals p{margin:5px 0;font-size:16px;}\n" +
            ".totals .grand-total{font-size:20px;font-weight:bold;color:#1a3c6e;margin-top:10px!important;}\n" +
            ".footer{text-align:center;color:#888;font-size:12px;margin-top:30px;padding-top:15px;border-top:1px solid #eee;}\n" +
            "</style></head><body>\n" +
            "<div class=\"container\">\n" +
            "  <div class=\"header\">\n" +
            "    <h1>AutoFlow Auto Repair</h1>\n" +
            "    <p>Your Trusted Auto Repair Partner | info@autoflow.com | (555) 123-4567</p>\n" +
            "  </div>\n" +
            "  <div class=\"invoice-meta\">\n" +
            "    <table><tr>\n" +
            "      <td>\n" +
            "        <p style=\"margin:0;color:#666;font-size:13px;\">INVOICE TO</p>\n" +
            "        <p style=\"margin:3px 0;font-size:16px;font-weight:bold;\">" + (sale.Customer?.FullName ?? "N/A") + "</p>\n" +
            "        <p style=\"margin:2px 0;color:#555;font-size:13px;\">" + (sale.Customer?.Email ?? "") + "</p>\n" +
            "        " + phoneLine + "\n" +
            "      </td>\n" +
            "      <td class=\"meta-right\">\n" +
            "        <h2>INVOICE</h2>\n" +
            "        <p style=\"margin:3px 0;\"><strong>#" + sale.InvoiceNumber + "</strong></p>\n" +
            "        <p style=\"margin:2px 0;color:#555;font-size:13px;\">Date: " + sale.SaleDate.ToString("MMMM dd, yyyy") + "</p>\n" +
            "        <p style=\"margin:2px 0;color:#555;font-size:13px;\">Payment: " + sale.PaymentMethod + "</p>\n" +
            "        " + staffLine + "\n" +
            "      </td>\n" +
            "    </tr></table>\n" +
            "  </div>\n" +
            "  <table class=\"items-table\">\n" +
            "    <thead><tr>\n" +
            "      <th style=\"padding:10px 8px;\">Item</th>\n" +
            "      <th style=\"padding:10px 8px;text-align:center;\">Qty</th>\n" +
            "      <th style=\"padding:10px 8px;text-align:right;\">Unit Price</th>\n" +
            "      <th style=\"padding:10px 8px;text-align:right;\">Subtotal</th>\n" +
            "    </tr></thead>\n" +
            "    <tbody>" + itemRows + "</tbody>\n" +
            "  </table>\n" +
            "  <div class=\"totals\">\n" +
            "    <p>Subtotal: $" + sale.SubTotal.ToString("F2") + "</p>\n" +
            "    " + discountLine + "\n" +
            "    <p class=\"grand-total\">Total: $" + sale.TotalAmount.ToString("F2") + "</p>\n" +
            "  </div>\n" +
            "  <div class=\"footer\">\n" +
            "    <p>Thank you for choosing AutoFlow Auto Repair!</p>\n" +
            "    <p>This invoice was generated automatically. For any questions, contact us at info@autoflow.com</p>\n" +
            "  </div>\n" +
            "</div>\n" +
            "</body></html>";
    }
}