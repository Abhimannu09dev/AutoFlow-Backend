using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AutoFlow_Backend.Infrastructure.Services;

public class InvoiceTemplateBuilder
{
    private readonly CompanySettings _companySettings;

    public InvoiceTemplateBuilder(IOptions<CompanySettings> companyOptions)
    {
        _companySettings = companyOptions.Value;
    }

    public string Build(SaleInvoiceDto invoice)
    {
        var itemRows = string.Join("", invoice.Items.Select(si =>
            "<tr>" +
            "<td style=\"padding:8px;border:1px solid #ddd;\">" + si.PartName + "</td>" +
            "<td style=\"padding:8px;border:1px solid #ddd;text-align:center;\">" + si.Quantity + "</td>" +
            "<td style=\"padding:8px;border:1px solid #ddd;text-align:right;\">$" + si.UnitPrice.ToString("F2") + "</td>" +
            "<td style=\"padding:8px;border:1px solid #ddd;text-align:right;\">$" + si.SubTotal.ToString("F2") + "</td>" +
            "</tr>"));

        var phoneLine = string.IsNullOrWhiteSpace(invoice.CustomerPhone)
            ? ""
            : "<p style=\"margin:2px 0;color:#555;font-size:13px;\">" + invoice.CustomerPhone + "</p>";
        var staffLine = string.IsNullOrWhiteSpace(invoice.StaffName)
            ? ""
            : "<p style=\"margin:2px 0;color:#555;font-size:13px;\">Issued by: " + invoice.StaffName + "</p>";
        var discountLine = invoice.DiscountAmount > 0
            ? "<p style=\"color:#28a745;\">Discount: -$" + invoice.DiscountAmount.ToString("F2") + "</p>"
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
            "    <h1>" + _companySettings.SenderName + " Auto Repair</h1>\n" +
            "    <p>Your Trusted Auto Repair Partner | " + _companySettings.ContactEmail + " | " + _companySettings.ContactPhone + "</p>\n" +
            "  </div>\n" +
            "  <div class=\"invoice-meta\">\n" +
            "    <table><tr>\n" +
            "      <td>\n" +
            "        <p style=\"margin:0;color:#666;font-size:13px;\">INVOICE TO</p>\n" +
            "        <p style=\"margin:3px 0;font-size:16px;font-weight:bold;\">" + invoice.CustomerName + "</p>\n" +
            "        <p style=\"margin:2px 0;color:#555;font-size:13px;\">" + invoice.CustomerEmail + "</p>\n" +
            "        " + phoneLine + "\n" +
            "      </td>\n" +
            "      <td class=\"meta-right\">\n" +
            "        <h2>INVOICE</h2>\n" +
            "        <p style=\"margin:3px 0;\"><strong>#" + invoice.InvoiceNumber + "</strong></p>\n" +
            "        <p style=\"margin:2px 0;color:#555;font-size:13px;\">Date: " + invoice.SaleDate.ToString("MMMM dd, yyyy") + "</p>\n" +
            "        <p style=\"margin:2px 0;color:#555;font-size:13px;\">Payment: " + invoice.PaymentMethod + "</p>\n" +
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
            "    <p>Subtotal: $" + invoice.SubTotal.ToString("F2") + "</p>\n" +
            "    " + discountLine + "\n" +
            "    <p class=\"grand-total\">Total: $" + invoice.TotalAmount.ToString("F2") + "</p>\n" +
            "  </div>\n" +
            "  <div class=\"footer\">\n" +
            "    <p>Thank you for choosing " + _companySettings.SenderName + " Auto Repair!</p>\n" +
            "    <p>This invoice was generated automatically. For any questions, contact us at " + _companySettings.ContactEmail + "</p>\n" +
            "  </div>\n" +
            "</div>\n" +
            "</body></html>";
    }
}