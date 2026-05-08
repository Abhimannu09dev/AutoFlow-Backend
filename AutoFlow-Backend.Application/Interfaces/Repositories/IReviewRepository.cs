using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IReviewRepository : IRepositoryBase<Review>
{
    Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default);
}