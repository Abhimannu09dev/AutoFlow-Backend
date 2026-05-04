using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Background.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Background.Infrastructure.Repositories;

public class SaleRepository(AppDbContext context)
    : RepositoryBase<Sale>(context), ISaleRepository
{
    public Task<List<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync(cancellationToken);
    }

    public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<Sale?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<List<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return Context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync(cancellationToken);
    }
}