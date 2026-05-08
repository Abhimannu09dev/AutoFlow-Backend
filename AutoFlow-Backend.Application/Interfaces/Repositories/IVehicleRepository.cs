using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IVehicleRepository : IRepositoryBase<Vehicle>
{
    Task<List<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Vehicle>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetUserIdsByVehicleQueryAsync(string normalizedLowerQuery, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
}