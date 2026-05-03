using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IPartRepository : IRepositoryBase<Part>
{
    Task<bool> ExistsActiveByPartNumberAsync(string normalizedPartNumber, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<string?> GetActiveVendorNameByIdAsync(Guid vendorId, CancellationToken cancellationToken = default);
    Task<List<Part>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Part?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Part?> GetActiveByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Part>> SearchActiveAsync(string? query, CancellationToken cancellationToken = default);
    Task<List<Part>> GetLowStockActiveAsync(CancellationToken cancellationToken = default);
}
