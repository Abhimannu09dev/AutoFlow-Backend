using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Credits;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AutoFlow_Backend.Application.Services;

public class CreditService : ICreditService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ICreditPaymentRepository _creditPaymentRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreditService> _logger;

    public CreditService(
        ISaleRepository saleRepository,
        ICreditPaymentRepository creditPaymentRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<CreditService> logger)
    {
        _saleRepository = saleRepository;
        _creditPaymentRepository = creditPaymentRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<CreditDetailResponse>> GetCreditDetailsAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdWithCreditPaymentsAsync(saleId, cancellationToken);
        if (sale is null)
            return ApiResponseFactory.FailNotFound<CreditDetailResponse>("Sale not found.");

        if (sale.PaymentMethod != PaymentMethod.Credit)
            return ApiResponseFactory.Fail<CreditDetailResponse>("Sale is not a credit sale.");

        var paidAmount = sale.CreditPayments.Sum(p => p.Amount);
        var remainingAmount = sale.TotalAmount - paidAmount;
        var daysOverdue = sale.DueDate.HasValue
            ? Math.Max(0, (int)(DateTime.UtcNow - sale.DueDate.Value).TotalDays)
            : 0;

        var status = sale.CreditStatus?.ToString() ?? "Outstanding";

        return ApiResponseFactory.Ok("Credit details retrieved successfully.", new CreditDetailResponse
        {
            SaleId = sale.Id,
            InvoiceNumber = sale.InvoiceNumber,
            CustomerId = sale.CustomerId,
            CustomerName = sale.Customer?.FullName ?? string.Empty,
            CustomerEmail = sale.Customer?.Email ?? string.Empty,
            CustomerPhone = sale.Customer?.Phone,
            SaleDate = sale.SaleDate,
            DueDate = sale.DueDate ?? sale.SaleDate,
            TotalCreditAmount = sale.TotalAmount,
            PaidAmount = paidAmount,
            RemainingAmount = remainingAmount,
            DaysOverdue = daysOverdue,
            Status = status,
            PaymentHistory = sale.CreditPayments.OrderBy(p => p.PaymentDate).Select(p => new CreditPaymentResponse
            {
                PaymentId = p.Id,
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod.ToString(),
                Note = p.Note
            }).ToList()
        });
    }

    public async Task<ApiResponse<RecordCreditPaymentResponse>> RecordPaymentAsync(
        Guid saleId,
        RecordCreditPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
            return ApiResponseFactory.Fail<RecordCreditPaymentResponse>("Payment amount must be greater than zero.");

        var sale = await _saleRepository.GetByIdWithCreditPaymentsAsync(saleId, cancellationToken);
        if (sale is null)
            return ApiResponseFactory.FailNotFound<RecordCreditPaymentResponse>("Sale not found.");

        if (sale.PaymentMethod != PaymentMethod.Credit)
            return ApiResponseFactory.Fail<RecordCreditPaymentResponse>("Sale is not a credit sale.");

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var payment = new CreditPayment
            {
                Id = Guid.NewGuid(),
                SaleId = saleId,
                Amount = request.Amount,
                PaymentDate = request.PaymentDate,
                PaymentMethod = request.PaymentMethod,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _creditPaymentRepository.AddAsync(payment, cancellationToken);
            await _creditPaymentRepository.SaveChangesAsync(cancellationToken);

            var existingPayments = sale.CreditPayments.ToList();
            existingPayments.Add(payment);

            var paidAmount = existingPayments.Sum(p => p.Amount);
            var remainingAmount = sale.TotalAmount - paidAmount;

            if (remainingAmount <= 0)
                sale.CreditStatus = CreditStatus.Paid;
            else if (paidAmount > 0)
                sale.CreditStatus = CreditStatus.PartiallyPaid;
            else
                sale.CreditStatus = CreditStatus.Outstanding;

            sale.UpdatedAt = DateTime.UtcNow;
            _saleRepository.Update(sale);
            await _saleRepository.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return ApiResponseFactory.Ok("Credit payment recorded successfully.", new RecordCreditPaymentResponse
            {
                SaleId = sale.Id,
                TotalCreditAmount = sale.TotalAmount,
                PaidAmount = paidAmount,
                RemainingAmount = remainingAmount,
                Status = sale.CreditStatus?.ToString() ?? "Outstanding",
                UpdatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record credit payment for sale {SaleId}", saleId);
            await transaction.RollbackAsync(cancellationToken);
            return ApiResponseFactory.Fail<RecordCreditPaymentResponse>("Failed to record payment. Please try again.");
        }
    }

    public async Task<ApiResponse<UpdateCreditStatusResponse>> UpdateStatusAsync(
        Guid saleId,
        UpdateCreditStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdForUpdateAsync(saleId, cancellationToken);
        if (sale is null)
            return ApiResponseFactory.FailNotFound<UpdateCreditStatusResponse>("Sale not found.");

        if (sale.PaymentMethod != PaymentMethod.Credit)
            return ApiResponseFactory.Fail<UpdateCreditStatusResponse>("Sale is not a credit sale.");

        sale.CreditStatus = request.Status;
        sale.UpdatedAt = DateTime.UtcNow;

        _saleRepository.Update(sale);
        await _saleRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Credit status updated successfully.", new UpdateCreditStatusResponse
        {
            SaleId = sale.Id,
            Status = sale.CreditStatus?.ToString() ?? "Outstanding",
            UpdatedAt = sale.UpdatedAt.Value
        });
    }

    public async Task<ApiResponse<SendCreditReminderResponse>> SendReminderAsync(
        Guid saleId,
        CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdWithCreditPaymentsAsync(saleId, cancellationToken);
        if (sale is null)
            return ApiResponseFactory.FailNotFound<SendCreditReminderResponse>("Sale not found.");

        if (sale.PaymentMethod != PaymentMethod.Credit)
            return ApiResponseFactory.Fail<SendCreditReminderResponse>("Sale is not a credit sale.");

        if (sale.Customer is null || string.IsNullOrWhiteSpace(sale.Customer.Email))
            return ApiResponseFactory.Fail<SendCreditReminderResponse>("Customer email not available. Reminder cannot be sent.");

        try
        {
            var dueDate = sale.DueDate ?? sale.SaleDate.AddDays(30);
            var paidAmount = sale.CreditPayments.Sum(p => p.Amount);
            var remainingAmount = Math.Max(0, sale.TotalAmount - paidAmount);
            var daysOverdue = Math.Max(0, (int)(DateTime.UtcNow.Date - dueDate.Date).TotalDays);
            var status = sale.CreditStatus?.ToString() ?? "Outstanding";

            var customerName = string.IsNullOrWhiteSpace(sale.Customer.FullName) ? "Customer" : sale.Customer.FullName;
            var subject = $"Credit Payment Reminder - {sale.InvoiceNumber}";
            string Escape(string value) =>
                value
                    .Replace("&", "&amp;", StringComparison.Ordinal)
                    .Replace("<", "&lt;", StringComparison.Ordinal)
                    .Replace(">", "&gt;", StringComparison.Ordinal)
                    .Replace("\"", "&quot;", StringComparison.Ordinal)
                    .Replace("'", "&#39;", StringComparison.Ordinal);

            var paymentRows = sale.CreditPayments
                .OrderBy(p => p.PaymentDate)
                .Select(p =>
                    "<tr>" +
                    "<td style=\"padding:10px;border:1px solid #e5e7eb;\">" + Escape(p.PaymentDate.ToString("MMM dd, yyyy")) + "</td>" +
                    "<td style=\"padding:10px;border:1px solid #e5e7eb;\">" + Escape(p.PaymentMethod.ToString()) + "</td>" +
                    "<td style=\"padding:10px;border:1px solid #e5e7eb;text-align:right;\">$" + p.Amount.ToString("F2") + "</td>" +
                    "<td style=\"padding:10px;border:1px solid #e5e7eb;\">" + Escape(p.Note ?? "-") + "</td>" +
                    "</tr>")
                .ToList();

            var paymentHistorySection = paymentRows.Count == 0
                ? "<p style=\"margin:0;color:#64748b;font-size:13px;\">No payments recorded yet.</p>"
                : "<table style=\"width:100%;border-collapse:collapse;margin-top:12px;\">" +
                  "<thead><tr style=\"background:#1a3c6e;color:#fff;\">" +
                  "<th style=\"padding:10px;text-align:left;\">Payment Date</th>" +
                  "<th style=\"padding:10px;text-align:left;\">Method</th>" +
                  "<th style=\"padding:10px;text-align:right;\">Amount</th>" +
                  "<th style=\"padding:10px;text-align:left;\">Note</th>" +
                  "</tr></thead><tbody>" + string.Join("", paymentRows) + "</tbody></table>";

            var customerEmail = string.IsNullOrWhiteSpace(sale.Customer.Email) ? "-" : sale.Customer.Email;
            var customerPhone = string.IsNullOrWhiteSpace(sale.Customer.Phone) ? "-" : sale.Customer.Phone;

            var body =
                "<!DOCTYPE html>" +
                "<html><head><meta charset=\"utf-8\"/>" +
                "<style>" +
                "body{font-family:Arial,sans-serif;margin:0;padding:20px;background:#f5f5f5;color:#1f2937;}" +
                ".container{max-width:900px;margin:0 auto;background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);}" +
                ".header{background:#183f73;color:#ffffff;text-align:center;padding:28px;}" +
                ".header h1{margin:0;font-size:30px;}" +
                ".header p{margin:8px 0 0;color:#dbeafe;font-size:13px;}" +
                ".section{padding:24px;}" +
                ".meta{background:#f8fafc;border:1px solid #e5e7eb;border-radius:8px;padding:20px;}" +
                ".meta table{width:100%;}" +
                ".meta td{vertical-align:top;}" +
                ".meta-title{color:#64748b;text-transform:uppercase;font-size:12px;letter-spacing:0.08em;margin:0;}" +
                ".meta-name{margin:6px 0 2px 0;font-size:20px;color:#0f172a;}" +
                ".meta-line{margin:2px 0;color:#475569;font-size:13px;}" +
                ".right{text-align:right;}" +
                ".right h2{margin:0 0 8px 0;font-size:24px;color:#183f73;}" +
                ".summary{width:100%;border-collapse:collapse;margin-top:22px;}" +
                ".summary th{background:#183f73;color:#ffffff;padding:12px;text-align:left;font-size:13px;}" +
                ".summary td{padding:12px;border:1px solid #e5e7eb;font-size:14px;}" +
                ".amount{text-align:right;}" +
                ".remaining{font-weight:700;color:#b91c1c;}" +
                ".balance{margin-top:18px;text-align:right;font-size:22px;color:#183f73;font-weight:700;}" +
                ".note{margin-top:14px;color:#334155;font-size:14px;}" +
                ".footer{padding:18px 24px;border-top:1px solid #e5e7eb;text-align:center;color:#64748b;font-size:12px;}" +
                "</style></head><body>" +
                "<div class=\"container\">" +
                "<div class=\"header\">" +
                "<h1>AutoFlow Auto Repair</h1>" +
                "<p>Your Trusted Auto Repair Partner | info@autoflow.com | (555) 123-4567</p>" +
                "</div>" +
                "<div class=\"section\">" +
                "<div class=\"meta\"><table><tr>" +
                "<td>" +
                "<p class=\"meta-title\">Reminder To</p>" +
                "<h3 class=\"meta-name\">" + Escape(customerName) + "</h3>" +
                "<p class=\"meta-line\">" + Escape(customerEmail) + "</p>" +
                "<p class=\"meta-line\">" + Escape(customerPhone) + "</p>" +
                "</td>" +
                "<td class=\"right\">" +
                "<h2>CREDIT PAYMENT REMINDER</h2>" +
                "<p class=\"meta-line\"><strong>Invoice #:</strong> " + Escape(sale.InvoiceNumber) + "</p>" +
                "<p class=\"meta-line\"><strong>Sale Date:</strong> " + Escape(sale.SaleDate.ToString("MMM dd, yyyy")) + "</p>" +
                "<p class=\"meta-line\"><strong>Due Date:</strong> " + Escape(dueDate.ToString("MMM dd, yyyy")) + "</p>" +
                "<p class=\"meta-line\"><strong>Status:</strong> " + Escape(status) + "</p>" +
                "</td>" +
                "</tr></table></div>" +
                "<table class=\"summary\">" +
                "<thead><tr><th>Description</th><th class=\"amount\">Amount</th></tr></thead>" +
                "<tbody>" +
                "<tr><td>Total Credit Amount</td><td class=\"amount\">$" + sale.TotalAmount.ToString("F2") + "</td></tr>" +
                "<tr><td>Paid Amount</td><td class=\"amount\">$" + paidAmount.ToString("F2") + "</td></tr>" +
                "<tr><td><strong>Remaining Amount</strong></td><td class=\"amount remaining\">$" + remainingAmount.ToString("F2") + "</td></tr>" +
                "</tbody></table>" +
                "<p class=\"balance\">Your remaining credit balance is $" + remainingAmount.ToString("F2") + "</p>" +
                "<p class=\"note\">Please make payment by the due date to avoid overdue status.</p>" +
                "<h4 style=\"margin:26px 0 8px 0;color:#183f73;\">Payment History</h4>" +
                paymentHistorySection +
                "</div>" +
                "<div class=\"footer\">" +
                "<p style=\"margin:0;\">This is a friendly reminder about your remaining credit balance.</p>" +
                "<p style=\"margin:6px 0 0 0;\">Thank you for choosing AutoFlow Auto Repair.</p>" +
                "</div>" +
                "</div></body></html>";

            await _emailService.SendAsync(sale.Customer.Email, subject, body, cancellationToken);

            return ApiResponseFactory.Ok("Credit reminder sent successfully.", new SendCreditReminderResponse
            {
                SaleId = sale.Id,
                SentAt = DateTime.UtcNow,
                Channel = "Email"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send credit reminder for sale {SaleId}", sale.Id);
            return ApiResponseFactory.Fail<SendCreditReminderResponse>("Reminder could not be sent. Try again later.");
        }
    }
}
