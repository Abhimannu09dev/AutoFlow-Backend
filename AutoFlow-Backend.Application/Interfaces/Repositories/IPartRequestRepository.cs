using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IPartRequestRepository : IRepositoryBase<PartRequest>
{
    Task<PagedResponse<PartRequest>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<PartRequest>> GetPagedByCustomerIdAsync(Guid customerId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<List<PartRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<PartRequest>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}