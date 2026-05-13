using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Common;
using AutoFlow_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Repositories;

public class ReviewRepository : RepositoryBase<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }

    public Task<PagedResponse<Review>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Review>()
            .AsNoTracking();

        query = (request.SortBy?.ToLower(), request.SortDir) switch
        {
            ("createdat", SortDirection.Desc) => query.OrderByDescending(r => r.CreatedAt),
            ("createdat", _)                  => query.OrderBy(r => r.CreatedAt),
            _                                => query.OrderByDescending(r => r.CreatedAt)
        };

        return PaginationHelper.ToPagedAsync(query, request, cancellationToken);
    }

    public async Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.Set<Review>()
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
}