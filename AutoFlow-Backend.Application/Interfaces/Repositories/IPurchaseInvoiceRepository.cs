using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IPurchaseInvoiceRepository : IRepositoryBase<PurchaseInvoice>
{
    Task<PagedResponse<PurchaseInvoice>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<List<PurchaseInvoice>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PurchaseInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PurchaseInvoice?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<PurchaseInvoice>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default);
}