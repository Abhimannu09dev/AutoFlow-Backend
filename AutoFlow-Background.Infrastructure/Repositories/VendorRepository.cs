using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Background.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Background.Infrastructure.Repositories;

public class VendorRepository(AppDbContext context)
    : RepositoryBase<Vendor>(context), IVendorRepository
{
    public Task<bool> ExistsActiveByNameAsync(
        string normalizedVendorName,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Vendors
            .AsNoTracking()
            .Where(v => v.IsActive && v.VendorName.ToLower() == normalizedVendorName);

        if (excludeId.HasValue)
        {
            query = query.Where(v => v.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<List<Vendor>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return Context.Vendors
            .AsNoTracking()
            .Where(v => v.IsActive)
            .OrderBy(v => v.VendorName)
            .ToListAsync(cancellationToken);
    }

    public Task<Vendor?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Vendors
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.IsActive && v.Id == id, cancellationToken);
    }

    public Task<Vendor?> GetActiveByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Vendors
            .FirstOrDefaultAsync(v => v.IsActive && v.Id == id, cancellationToken);
    }

    public Task<List<Vendor>> SearchActiveAsync(string? query, CancellationToken cancellationToken = default)
    {
        var vendorQuery = Context.Vendors
            .AsNoTracking()
            .Where(v => v.IsActive);

        var normalizedQuery = query?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            var lowered = normalizedQuery.ToLowerInvariant();
            vendorQuery = vendorQuery.Where(v =>
                v.VendorName.ToLower().Contains(lowered) ||
                v.Phone.ToLower().Contains(lowered) ||
                (v.ContactPerson != null && v.ContactPerson.ToLower().Contains(lowered)) ||
                (v.Email != null && v.Email.ToLower().Contains(lowered)));
        }

        return vendorQuery
            .OrderBy(v => v.VendorName)
            .ToListAsync(cancellationToken);
    }
}
