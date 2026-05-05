using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PurchaseInvoices;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.Services;

public class PurchaseInvoiceService : IPurchaseInvoiceService
{
    private readonly IPurchaseInvoiceRepository _purchaseInvoiceRepository;
    private readonly IPartRepository _partRepository;
    private readonly IVendorRepository _vendorRepository;

    public PurchaseInvoiceService(
        IPurchaseInvoiceRepository purchaseInvoiceRepository,
        IPartRepository partRepository,
        IVendorRepository vendorRepository)
    {
        _purchaseInvoiceRepository = purchaseInvoiceRepository;
        _partRepository = partRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task<ApiResponse<PurchaseInvoiceResponse>> CreateAsync(
        CreatePurchaseInvoiceRequest request,
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate items list is not empty
        if (request.Items == null || request.Items.Count == 0)
            return Fail<PurchaseInvoiceResponse>("Purchase invoice must have at least one item.");

        // 2. Validate vendor exists
        var vendor = await _vendorRepository.GetActiveByIdAsync(request.VendorId, cancellationToken);
        if (vendor is null)
            return Fail<PurchaseInvoiceResponse>("Vendor not found.");

        // 3. Validate each part and quantity
        var resolvedItems = new List<(PurchaseInvoiceItemRequest Request, Part Part)>();

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return Fail<PurchaseInvoiceResponse>($"Quantity for part {item.PartId} must be greater than zero.");

            if (item.UnitCost < 0)
                return Fail<PurchaseInvoiceResponse>($"Unit cost for part {item.PartId} cannot be negative.");

            var part = await _partRepository.GetActiveByIdForUpdateAsync(item.PartId, cancellationToken);
            if (part is null)
                return Fail<PurchaseInvoiceResponse>($"Part {item.PartId} not found.");

            resolvedItems.Add((item, part));
        }

        // 4. Build invoice items + calculate total
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

        // 5. Increment stock for each part
        foreach (var (item, part) in resolvedItems)
        {
            part.StockQuantity += item.Quantity;
            part.UpdatedAt = DateTime.UtcNow;
            _partRepository.Update(part);
        }

        // 6. Build and save the invoice
        var invoice = new PurchaseInvoice
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

        return Success("Purchase invoice created successfully.", MapToResponse(invoice, vendor.VendorName));
    }

    public async Task<ApiResponse<List<PurchaseInvoiceResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _purchaseInvoiceRepository.GetAllAsync(cancellationToken);
        return Success("Purchase invoices retrieved successfully.", invoices.Select(MapToResponse).ToList());
    }

    public async Task<ApiResponse<PurchaseInvoiceResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _purchaseInvoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null)
            return Fail<PurchaseInvoiceResponse>("Purchase invoice not found.");

        return Success("Purchase invoice retrieved successfully.", MapToResponse(invoice));
    }

    public async Task<ApiResponse<List<PurchaseInvoiceResponse>>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        var invoices = await _purchaseInvoiceRepository.GetByVendorIdAsync(vendorId, cancellationToken);
        return Success("Purchase invoices retrieved successfully.", invoices.Select(MapToResponse).ToList());
    }

    private static PurchaseInvoiceResponse MapToResponse(PurchaseInvoice invoice)
    {
        return MapToResponse(invoice, invoice.Vendor?.VendorName ?? string.Empty);
    }

    private static PurchaseInvoiceResponse MapToResponse(PurchaseInvoice invoice, string vendorName)
    {
        return new PurchaseInvoiceResponse
        {
            Id = invoice.Id,
            VendorId = invoice.VendorId,
            VendorName = vendorName,
            CreatedByStaffId = invoice.CreatedByStaffId,
            InvoiceDate = invoice.InvoiceDate,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            Items = invoice.Items.Select(i => new PurchaseInvoiceItemResponse
            {
                Id = i.Id,
                PartId = i.PartId,
                PartName = i.Part?.PartName ?? string.Empty,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                SubTotal = i.SubTotal
            }).ToList()
        };
    }

    private static ApiResponse<T> Success<T>(string message, T data) =>
        new() { Status = true, Message = message, Data = data };

    private static ApiResponse<T> Fail<T>(string message) =>
        new() { Status = false, Message = message, Data = default };
}