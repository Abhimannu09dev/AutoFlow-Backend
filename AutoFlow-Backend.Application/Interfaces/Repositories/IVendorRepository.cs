using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IVendorRepository : IRepositoryBase<Vendor>
{
    Task<PagedResponse<Vendor>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveByNameAsync(string normalizedVendorName, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string normalizedEmail, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<List<Vendor>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Vendor?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vendor?> GetActiveByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Vendor>> SearchActiveAsync(string? query, CancellationToken cancellationToken = default);
}
