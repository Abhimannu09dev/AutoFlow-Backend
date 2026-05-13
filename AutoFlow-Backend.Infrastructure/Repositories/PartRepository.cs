using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Common;
using AutoFlow_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Repositories;

public class PartRepository(AppDbContext context)
    : RepositoryBase<Part>(context), IPartRepository
{
    public Task<PagedResponse<Part>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = Context.Parts
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Where(p => p.IsActive)
            .OrderBy(p => p.PartName);

        return PaginationHelper.ToPagedAsync(query, request, cancellationToken);
    }

    public Task<bool> ExistsActiveByPartNumberAsync(
        string normalizedPartNumber,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Parts
            .AsNoTracking()
            .Where(p => p.IsActive && p.PartNumber.ToLower() == normalizedPartNumber);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<string?> GetActiveVendorNameByIdAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        return Context.Vendors
            .AsNoTracking()
            .Where(v => v.IsActive && v.Id == vendorId)
            .Select(v => v.VendorName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<Part>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return Context.Parts
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Where(p => p.IsActive)
            .OrderBy(p => p.PartName)
            .ToListAsync(cancellationToken);
    }

    public Task<Part?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Parts
            .AsNoTracking()
            .Include(p => p.Vendor)
            .FirstOrDefaultAsync(p => p.IsActive && p.Id == id, cancellationToken);
    }

    public Task<Part?> GetActiveByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Parts
            .Include(p => p.Vendor)
            .FirstOrDefaultAsync(p => p.IsActive && p.Id == id, cancellationToken);
    }

    public Task<List<Part>> SearchActiveAsync(string? query, CancellationToken cancellationToken = default)
    {
        var partQuery = Context.Parts
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Where(p => p.IsActive);

        var normalizedQuery = query?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            var lowered = normalizedQuery.ToLowerInvariant();
            partQuery = partQuery.Where(p =>
                p.PartName.ToLower().Contains(lowered) ||
                p.PartNumber.ToLower().Contains(lowered) ||
                (p.Brand != null && p.Brand.ToLower().Contains(lowered)) ||
                (p.Category != null && p.Category.ToLower().Contains(lowered)));
        }

        return partQuery
            .OrderBy(p => p.PartName)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Part>> GetLowStockActiveAsync(CancellationToken cancellationToken = default)
    {
        return Context.Parts
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Where(p => p.IsActive && p.StockQuantity < p.MinimumStockLevel)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.PartName)
            .ToListAsync(cancellationToken);
    }
}
