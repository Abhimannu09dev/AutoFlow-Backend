using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.Services;

public class SaleService : ISaleService
{
    private const decimal LoyaltyDiscountThreshold = 5000m;
    private const decimal LoyaltyDiscountRate = 0.10m;

    private readonly ISaleRepository _saleRepository;
    private readonly IPartRepository _partRepository;
    private readonly IEmailService _emailService;

    public SaleService(
        ISaleRepository saleRepository,
        IPartRepository partRepository,
        IEmailService emailService)
    {
        _saleRepository = saleRepository;
        _partRepository = partRepository;
        _emailService = emailService;
    }

    public async Task<ApiResponse<SaleResponse>> CreateAsync(
        CreateSaleRequest request,
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        if (request.Items == null || request.Items.Count == 0)
            return ApiResponseFactory.Fail<SaleResponse>("Sale must have at least one item.");

        var resolvedItems = new List<(SaleItemRequest Request, Part Part)>();

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return ApiResponseFactory.Fail<SaleResponse>($"Quantity for part {item.PartId} must be greater than zero.");

            var part = await _partRepository.GetActiveByIdForUpdateAsync(item.PartId, cancellationToken);
            if (part is null)
                return ApiResponseFactory.Fail<SaleResponse>($"Part {item.PartId} not found."); 
            if (part.StockQuantity < item.Quantity)
                return ApiResponseFactory.Fail<SaleResponse>($"Insufficient stock for part '{part.PartName}'. Available: {part.StockQuantity}, Requested: {item.Quantity}.");

            resolvedItems.Add((item, part));
        }

        var saleItems = new List<SaleItem>();
        decimal subTotal = 0;

        foreach (var (item, part) in resolvedItems)
        {
            var lineSubTotal = part.SellingPrice * item.Quantity;
            subTotal += lineSubTotal;

            saleItems.Add(new SaleItem
            {
                Id = Guid.NewGuid(),
                PartId = part.Id,
                Quantity = item.Quantity,
                UnitPrice = part.SellingPrice,
                SubTotal = lineSubTotal
            });
        }

        decimal discountAmount = 0;
        if (subTotal > LoyaltyDiscountThreshold)
            discountAmount = Math.Round(subTotal * LoyaltyDiscountRate, 2);

        var totalAmount = subTotal - discountAmount;

        foreach (var (item, part) in resolvedItems)
        {
            part.StockQuantity -= item.Quantity;
            part.UpdatedAt = DateTime.UtcNow;
            _partRepository.Update(part);
        }

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            StaffId = staffId,
            InvoiceNumber = GenerateInvoiceNumber(),
            SaleDate = DateTime.UtcNow,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            PaymentMethod = request.PaymentMethod,
            Status = SaleStatus.Completed,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            SaleItems = saleItems
        };

        await _saleRepository.AddAsync(sale, cancellationToken);
        await _saleRepository.SaveChangesAsync(cancellationToken);

        await TrySendInvoiceAsync(sale, cancellationToken);

        return ApiResponseFactory.Ok("Sale created successfully.", MapToResponse(sale));
    }

    public async Task<ApiResponse<List<SaleResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sales = await _saleRepository.GetAllAsync(cancellationToken);
        return ApiResponseFactory.Ok("Sales retrieved successfully.", sales.Select(MapToResponse).ToList());
    }

    public async Task<ApiResponse<SaleResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdAsync(id, cancellationToken);
        if (sale is null)
            return ApiResponseFactory.Fail<SaleResponse>("Sale not found.");

        return ApiResponseFactory.Ok("Sale retrieved successfully.", MapToResponse(sale));
    }

    public async Task<ApiResponse<List<SaleResponse>>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var sales = await _saleRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return ApiResponseFactory.Ok("Sales retrieved successfully.", sales.Select(MapToResponse).ToList());
    }

    public async Task<ApiResponse<bool>> SendInvoiceAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdForInvoiceAsync(saleId, cancellationToken);
        if (sale is null)
            return ApiResponseFactory.FailNotFound<bool>("Sale not found.");

        if (sale.Customer is null || string.IsNullOrWhiteSpace(sale.Customer.Email))
            return ApiResponseFactory.Fail<bool>("Customer email not available. Invoice cannot be sent.");

        var html = BuildInvoiceHtml(sale);
        var subject = $"AutoFlow Invoice #{sale.InvoiceNumber}";
        await _emailService.SendAsync(sale.Customer.Email, subject, html, cancellationToken);

        sale.InvoiceSentAt = DateTime.UtcNow;
        sale.InvoiceEmail = sale.Customer.Email;
        _saleRepository.Update(sale);
        await _saleRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok($"Invoice sent to {sale.Customer.Email}.", true);
    }

    private async Task TrySendInvoiceAsync(Sale sale, CancellationToken cancellationToken)
    {
        try
        {
            if (sale.Customer is null || string.IsNullOrWhiteSpace(sale.Customer.Email))
                return;

            var html = BuildInvoiceHtml(sale);
            var subject = $"AutoFlow Invoice #{sale.InvoiceNumber}";
            await _emailService.SendAsync(sale.Customer.Email, subject, html, cancellationToken);

            sale.InvoiceSentAt = DateTime.UtcNow;
            sale.InvoiceEmail = sale.Customer.Email;
            _saleRepository.Update(sale);
            await _saleRepository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
        }
    }

    private static string GenerateInvoiceNumber() =>
        $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

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

    private static SaleResponse MapToResponse(Sale sale)
    {
        return new SaleResponse
        {
            Id = sale.Id,
            InvoiceNumber = sale.InvoiceNumber,
            CustomerId = sale.CustomerId,
            CustomerName = sale.Customer is not null
                ? sale.Customer.FullName
                : string.Empty,
            StaffId = sale.StaffId,
            SaleDate = sale.SaleDate,
            SubTotal = sale.SubTotal,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            LoyaltyDiscountApplied = sale.DiscountAmount > 0,
            PaymentMethod = sale.PaymentMethod,
            Status = sale.Status,
            Notes = sale.Notes,
            InvoiceSentAt = sale.InvoiceSentAt,
            CreatedAt = sale.CreatedAt,
            Items = sale.SaleItems.Select(si => new SaleItemResponse
            {
                Id = si.Id,
                PartId = si.PartId,
                PartName = si.Part?.PartName ?? string.Empty,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                SubTotal = si.SubTotal
            }).ToList()
        };
    }
}