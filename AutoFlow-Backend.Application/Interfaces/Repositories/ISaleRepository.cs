using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface ISaleRepository : IRepositoryBase<Sale>
{
    Task<PagedResponse<Sale>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<List<Sale>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdForInvoiceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdWithCreditPaymentsAsync(Guid id, CancellationToken cancellationToken = default);
}