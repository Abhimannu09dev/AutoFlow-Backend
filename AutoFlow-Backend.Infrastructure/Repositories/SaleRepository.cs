using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Common;
using AutoFlow_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Repositories;

public class SaleRepository(AppDbContext context)
    : RepositoryBase<Sale>(context), ISaleRepository
{
    public Task<PagedResponse<Sale>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        IQueryable<Sale> query = Context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Staff)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part);

        query = (request.SortBy?.ToLower(), request.SortDir) switch
        {
            ("createdat", SortDirection.Desc) => query.OrderByDescending(s => s.CreatedAt),
            ("createdat", _)                  => query.OrderBy(s => s.CreatedAt),
            _                                => query.OrderByDescending(s => s.SaleDate)
        };

        return PaginationHelper.ToPagedAsync(query, request, cancellationToken);
    }

    public Task<List<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Staff)
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
            .Include(s => s.Staff)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<Sale?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Staff)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<List<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return Context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Staff)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync(cancellationToken);
    }

    public Task<Sale?> GetByIdForInvoiceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Staff)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Part)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<Sale?> GetByIdWithCreditPaymentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Sales
            .Include(s => s.Customer)
            .Include(s => s.CreditPayments)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}