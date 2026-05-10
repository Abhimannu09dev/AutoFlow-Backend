using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Sales;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Application.Mappers;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoFlow_Backend.Application.Services;

public class SaleService : ISaleService
{
    private readonly BusinessRulesSettings _businessRules;

    private readonly ISaleRepository _saleRepository;
    private readonly IPartRepository _partRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<SaleService> _logger;
    private readonly DbContext _context;

    public SaleService(
        ISaleRepository saleRepository,
        IPartRepository partRepository,
        IEmailService emailService,
        ILogger<SaleService> logger,
        DbContext context,
        IOptions<BusinessRulesSettings> businessRules)
    {
        _saleRepository = saleRepository;
        _partRepository = partRepository;
        _emailService = emailService;
        _logger = logger;
        _context = context;
        _businessRules = businessRules.Value;
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
        if (subTotal > _businessRules.LoyaltyDiscountThreshold)
            discountAmount = Math.Round(subTotal * _businessRules.LoyaltyDiscountRate, 2);

        var totalAmount = subTotal - discountAmount;

        Sale? sale = null;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var (item, part) in resolvedItems)
            {
                part.StockQuantity -= item.Quantity;
                part.UpdatedAt = DateTime.UtcNow;
                _partRepository.Update(part);
            }

            sale = new Sale
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

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        await TrySendInvoiceAsync(sale!, cancellationToken);

        return ApiResponseFactory.Ok("Sale created successfully.", SaleMapper.ToResponse(sale!));
    }

    public async Task<ApiResponse<List<SaleResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sales = await _saleRepository.GetAllAsync(cancellationToken);
        return ApiResponseFactory.Ok("Sales retrieved successfully.", sales.Select(SaleMapper.ToResponse).ToList());
    }

    public async Task<ApiResponse<SaleResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdAsync(id, cancellationToken);
        if (sale is null)
            return ApiResponseFactory.Fail<SaleResponse>("Sale not found.");

        return ApiResponseFactory.Ok("Sale retrieved successfully.", SaleMapper.ToResponse(sale));
    }

    public async Task<ApiResponse<List<SaleResponse>>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var sales = await _saleRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return ApiResponseFactory.Ok("Sales retrieved successfully.", sales.Select(SaleMapper.ToResponse).ToList());
    }

    public async Task<ApiResponse<bool>> SendInvoiceAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdForInvoiceAsync(saleId, cancellationToken);
        if (sale is null)
            return ApiResponseFactory.FailNotFound<bool>("Sale not found.");

        if (sale.Customer is null || string.IsNullOrWhiteSpace(sale.Customer.Email))
            return ApiResponseFactory.Fail<bool>("Customer email not available. Invoice cannot be sent.");

        var invoiceDto = SaleMapper.ToInvoiceDto(sale);
        await _emailService.SendInvoiceAsync(invoiceDto, cancellationToken);

        sale.InvoiceSentAt = DateTime.UtcNow;
        sale.InvoiceEmail = sale.Customer.Email;
        sale.InvoiceFailedAt = null;
        sale.InvoiceFailureReason = null;
        _saleRepository.Update(sale);
        await _saleRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok($"Invoice sent to {sale.Customer.Email}.", true);
    }

    private async Task TrySendInvoiceAsync(Sale sale, CancellationToken cancellationToken)
    {
        if (sale.Customer is null || string.IsNullOrWhiteSpace(sale.Customer.Email))
            return;

        try
        {
            var invoiceDto = SaleMapper.ToInvoiceDto(sale);
            await _emailService.SendInvoiceAsync(invoiceDto, cancellationToken);

            sale.InvoiceSentAt = DateTime.UtcNow;
            sale.InvoiceEmail = sale.Customer.Email;
            sale.InvoiceFailedAt = null;
            sale.InvoiceFailureReason = null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Invoice email failed for sale {SaleId} (invoice {InvoiceNumber}) to {Email}",
                sale.Id, sale.InvoiceNumber, sale.Customer.Email);

            sale.InvoiceFailedAt = DateTime.UtcNow;
            sale.InvoiceFailureReason = ex.Message.Length > 500
                ? ex.Message[..500]
                : ex.Message;
        }
        finally
        {
            _saleRepository.Update(sale);
            await _saleRepository.SaveChangesAsync(cancellationToken);
        }
    }

    private static string GenerateInvoiceNumber() =>
        $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
}