using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface ISaleRepository : IRepositoryBase<Sale>
{
    Task<List<Sale>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}