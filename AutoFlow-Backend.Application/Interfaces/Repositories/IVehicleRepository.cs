using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IVehicleRepository : IRepositoryBase<Vehicle>
{
    Task<List<Vehicle>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetUserIdsByVehicleQueryAsync(string normalizedLowerQuery, CancellationToken cancellationToken = default);
}