using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.PurchaseInvoices;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IPurchaseInvoiceService
{
    Task<ApiResponse<PurchaseInvoiceResponse>> CreateAsync(CreatePurchaseInvoiceRequest request, Guid staffId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PurchaseInvoiceResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PurchaseInvoiceResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PurchaseInvoiceResponse>>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default);
}