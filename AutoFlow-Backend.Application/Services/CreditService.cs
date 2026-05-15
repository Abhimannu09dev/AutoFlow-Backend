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
        var sale = await _saleRepository.GetByIdForInvoiceAsync(saleId, cancellationToken);
        if (sale is null)
            return ApiResponseFactory.FailNotFound<SendCreditReminderResponse>("Sale not found.");

        if (sale.PaymentMethod != PaymentMethod.Credit)
            return ApiResponseFactory.Fail<SendCreditReminderResponse>("Sale is not a credit sale.");

        if (sale.Customer is null || string.IsNullOrWhiteSpace(sale.Customer.Email))
            return ApiResponseFactory.Fail<SendCreditReminderResponse>("Customer email not available. Reminder cannot be sent.");

        try
        {
            var subject = $"Credit Payment Reminder - {sale.InvoiceNumber}";
            var body = $"Dear {sale.Customer.FullName},\n\nThis is a reminder that credit payment for invoice {sale.InvoiceNumber} is due.\n\nAmount: ${sale.TotalAmount:F2}\nDue Date: {sale.DueDate:yyyy-MM-dd}\n\nThank you for your business.";

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
