using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface ICustomerRepository : IRepositoryBase<Customer>
{
    Task<PagedResponse<Customer>> GetPagedAsync(DTOs.Customers.CustomerPagedRequest request, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string normalizedEmail, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Customer>> SearchAsync(string normalizedLowerQuery, IReadOnlyList<Guid> matchingUserIds, Guid? customerIdMatch, CancellationToken cancellationToken = default);
    Task<Customer?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByApplicationUserIdForUpdateAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
    Task<List<Customer>> GetByApplicationUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}