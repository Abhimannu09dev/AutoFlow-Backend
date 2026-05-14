using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Sales;

namespace AutoFlow_Backend.Application.Interfaces;

public interface ISaleService
{
    Task<ApiResponse<SaleResponse>> CreateAsync(CreateSaleRequest request, Guid staffId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<SaleResponse>>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SaleResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<SaleResponse>>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SendInvoiceResponse>> SendInvoiceAsync(Guid saleId, CancellationToken cancellationToken = default);
}