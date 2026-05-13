using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IReviewRepository : IRepositoryBase<Review>
{
    Task<PagedResponse<Review>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default);
}