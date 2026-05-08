using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Repositories;

public class ReviewRepository : RepositoryBase<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }

    public async Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.Set<Review>()
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
}