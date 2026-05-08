using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IPartRequestRepository : IRepositoryBase<PartRequest>
{
    Task<List<PartRequest>> GetAllAsync(CancellationToken cancellationToken = default);
}