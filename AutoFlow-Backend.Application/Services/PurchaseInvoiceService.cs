using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PurchaseInvoices;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Application.Mappers;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Services;

public class PurchaseInvoiceService : IPurchaseInvoiceService
{
    private readonly IPurchaseInvoiceRepository _purchaseInvoiceRepository;
    private readonly IPartRepository _partRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly DbContext _context;

    public PurchaseInvoiceService(
        IPurchaseInvoiceRepository purchaseInvoiceRepository,
        IPartRepository partRepository,
        IVendorRepository vendorRepository,
        DbContext context)
    {
        _purchaseInvoiceRepository = purchaseInvoiceRepository;
        _partRepository = partRepository;
        _vendorRepository = vendorRepository;
        _context = context;
    }

    public async Task<ApiResponse<PurchaseInvoiceResponse>> CreateAsync(
        CreatePurchaseInvoiceRequest request,
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        if (request.Items == null || request.Items.Count == 0)
            return ApiResponseFactory.Fail<PurchaseInvoiceResponse>("Purchase invoice must have at least one item.");

        var vendor = await _vendorRepository.GetActiveByIdAsync(request.VendorId, cancellationToken);
        if (vendor is null)
            return ApiResponseFactory.Fail<PurchaseInvoiceResponse>("Vendor not found.");
        var resolvedItems = new List<(PurchaseInvoiceItemRequest Request, Part Part)>();

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return ApiResponseFactory.Fail<PurchaseInvoiceResponse>($"Quantity for part {item.PartId} must be greater than zero.");

            if (item.UnitCost < 0)
                return ApiResponseFactory.Fail<PurchaseInvoiceResponse>($"Unit cost for part {item.PartId} cannot be negative.");

            var part = await _partRepository.GetActiveByIdForUpdateAsync(item.PartId, cancellationToken);
            if (part is null)
                return ApiResponseFactory.Fail<PurchaseInvoiceResponse>($"Part {item.PartId} not found.");
            resolvedItems.Add((item, part));
        }

        var invoiceItems = new List<PurchaseInvoiceItem>();
        decimal totalAmount = 0;

        foreach (var (item, part) in resolvedItems)
        {
            var lineSubTotal = item.UnitCost * item.Quantity;
            totalAmount += lineSubTotal;

            invoiceItems.Add(new PurchaseInvoiceItem
            {
                Id = Guid.NewGuid(),
                PartId = part.Id,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                SubTotal = lineSubTotal
            });
        }

        PurchaseInvoice? invoice = null;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var (item, part) in resolvedItems)
            {
                part.StockQuantity += item.Quantity;
                part.UpdatedAt = DateTime.UtcNow;
                _partRepository.Update(part);
            }

            invoice = new PurchaseInvoice
            {
                Id = Guid.NewGuid(),
                VendorId = request.VendorId,
                CreatedByStaffId = staffId,
                InvoiceDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = PurchaseInvoiceStatus.Received,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                CreatedAt = DateTime.UtcNow,
                Items = invoiceItems
            };

            await _purchaseInvoiceRepository.AddAsync(invoice, cancellationToken);
            await _purchaseInvoiceRepository.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return ApiResponseFactory.Ok("Purchase invoice created successfully.", PurchaseInvoiceMapper.ToResponse(invoice!, vendor.VendorName));
    }

    public async Task<ApiResponse<List<PurchaseInvoiceResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _purchaseInvoiceRepository.GetAllAsync(cancellationToken);
        return ApiResponseFactory.Ok("Purchase invoices retrieved successfully.", invoices.Select(PurchaseInvoiceMapper.ToResponse).ToList());
    }

    public async Task<ApiResponse<PurchaseInvoiceResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _purchaseInvoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null)
            return ApiResponseFactory.Fail<PurchaseInvoiceResponse>("Purchase invoice not found.");

        return ApiResponseFactory.Ok("Purchase invoice retrieved successfully.", PurchaseInvoiceMapper.ToResponse(invoice));
    }

    public async Task<ApiResponse<List<PurchaseInvoiceResponse>>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        var invoices = await _purchaseInvoiceRepository.GetByVendorIdAsync(vendorId, cancellationToken);
        return ApiResponseFactory.Ok("Purchase invoices retrieved successfully.", invoices.Select(PurchaseInvoiceMapper.ToResponse).ToList());
    }
}