using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Common;
using AutoFlow_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Repositories;

public class VehicleRepository : RepositoryBase<Vehicle>, IVehicleRepository
{
    public VehicleRepository(AppDbContext context) : base(context) { }

    public Task<PagedResponse<Vehicle>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Vehicle>()
            .AsNoTracking()
            .OrderBy(v => v.VehicleNumber);

        return PaginationHelper.ToPagedAsync(query, request, cancellationToken);
    }

    public Task<PagedResponse<Vehicle>> GetPagedByUserIdAsync(Guid userId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Vehicle>()
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.VehicleNumber);

        return PaginationHelper.ToPagedAsync(query, request, cancellationToken);
    }

    public async Task<List<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.Set<Vehicle>()
            .AsNoTracking()
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<Vehicle>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await Context.Set<Vehicle>()
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<Guid>> GetUserIdsByVehicleQueryAsync(
        string normalizedLowerQuery,
        CancellationToken cancellationToken = default) =>
        await Context.Set<Vehicle>()
            .AsNoTracking()
            .Where(v =>
                v.VehicleNumber.ToLower().Contains(normalizedLowerQuery) ||
                v.Brand.ToLower().Contains(normalizedLowerQuery) ||
                v.Model.ToLower().Contains(normalizedLowerQuery))
            .Select(v => v.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Set<Vehicle>()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<Vehicle?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Set<Vehicle>()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
}