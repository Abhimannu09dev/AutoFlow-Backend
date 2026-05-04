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

    public SaleService(ISaleRepository saleRepository, IPartRepository partRepository)
    {
        _saleRepository = saleRepository;
        _partRepository = partRepository;
    }

    public async Task<ApiResponse<SaleResponse>> CreateAsync(
        CreateSaleRequest request,
        Guid staffId,
        CancellationToken cancellationToken = default)
    {
        if (request.Items == null || request.Items.Count == 0)
            return Fail<SaleResponse>("Sale must have at least one item.");

        var resolvedItems = new List<(SaleItemRequest Request, Part Part)>();

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return Fail<SaleResponse>($"Quantity for part {item.PartId} must be greater than zero.");

            var part = await _partRepository.GetActiveByIdForUpdateAsync(item.PartId, cancellationToken);
            if (part is null)
                return Fail<SaleResponse>($"Part {item.PartId} not found.");

            if (part.StockQuantity < item.Quantity)
                return Fail<SaleResponse>($"Insufficient stock for part '{part.PartName}'. Available: {part.StockQuantity}, Requested: {item.Quantity}.");

            resolvedItems.Add((item, part));
        }

        var saleItems = new List<SaleItems>();
        decimal subTotal = 0;

        foreach (var (item, part) in resolvedItems)
        {
            var lineSubTotal = part.SellingPrice * item.Quantity;
            subTotal += lineSubTotal;

            saleItems.Add(new SaleItems
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

        return Success("Sale created successfully.", MapToResponse(sale));
    }

    public async Task<ApiResponse<List<SaleResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sales = await _saleRepository.GetAllAsync(cancellationToken);
        return Success("Sales retrieved successfully.", sales.Select(MapToResponse).ToList());
    }

    public async Task<ApiResponse<SaleResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdAsync(id, cancellationToken);
        if (sale is null)
            return Fail<SaleResponse>("Sale not found.");

        return Success("Sale retrieved successfully.", MapToResponse(sale));
    }

    public async Task<ApiResponse<List<SaleResponse>>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var sales = await _saleRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return Success("Sales retrieved successfully.", sales.Select(MapToResponse).ToList());
    }

    private static SaleResponse MapToResponse(Sale sale)
    {
        return new SaleResponse
        {
            Id = sale.Id,
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

    private static ApiResponse<T> Success<T>(string message, T data) =>
        new() { Status = true, Message = message, Data = data };

    private static ApiResponse<T> Fail<T>(string message) =>
        new() { Status = false, Message = message, Data = default };
}