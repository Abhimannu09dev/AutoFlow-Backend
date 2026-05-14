using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Common;
using AutoFlow_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Repositories;

public class PartRequestRepository : RepositoryBase<PartRequest>, IPartRequestRepository
{
    public PartRequestRepository(AppDbContext context) : base(context) { }

    public Task<PagedResponse<PartRequest>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<PartRequest>()
            .AsNoTracking()
            .OrderByDescending(pr => pr.CreatedAt);

        return PaginationHelper.ToPagedAsync(query, request, cancellationToken);
    }

    public Task<PagedResponse<PartRequest>> GetPagedByCustomerIdAsync(Guid customerId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<PartRequest>()
            .AsNoTracking()
            .Where(pr => pr.CustomerId == customerId)
            .OrderByDescending(pr => pr.CreatedAt);

        return PaginationHelper.ToPagedAsync(query, request, cancellationToken);
    }

    public async Task<List<PartRequest>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.Set<PartRequest>()
            .AsNoTracking()
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<PartRequest>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        await Context.Set<PartRequest>()
            .AsNoTracking()
            .Where(pr => pr.CustomerId == customerId)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<PartRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Set<PartRequest>()
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);
}
