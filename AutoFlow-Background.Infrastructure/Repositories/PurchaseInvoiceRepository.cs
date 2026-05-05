using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Background.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Background.Infrastructure.Repositories;

public class PurchaseInvoiceRepository(AppDbContext context)
    : RepositoryBase<PurchaseInvoice>(context), IPurchaseInvoiceRepository
{
    public Task<List<PurchaseInvoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Context.PurchaseInvoices
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.Items)
                .ThenInclude(i => i.Part)
            .OrderByDescending(p => p.InvoiceDate)
            .ToListAsync(cancellationToken);
    }

    public Task<PurchaseInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.PurchaseInvoices
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.Items)
                .ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<PurchaseInvoice?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.PurchaseInvoices
            .Include(p => p.Vendor)
            .Include(p => p.Items)
                .ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<List<PurchaseInvoice>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        return Context.PurchaseInvoices
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.Items)
                .ThenInclude(i => i.Part)
            .Where(p => p.VendorId == vendorId)
            .OrderByDescending(p => p.InvoiceDate)
            .ToListAsync(cancellationToken);
    }
}