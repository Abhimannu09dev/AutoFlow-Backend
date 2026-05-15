using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Repositories;

public class CreditPaymentRepository(AppDbContext context)
    : RepositoryBase<CreditPayment>(context), ICreditPaymentRepository
{
    public async Task<List<CreditPayment>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        return await Context.CreditPayments
            .AsNoTracking()
            .Where(cp => cp.SaleId == saleId)
            .OrderBy(cp => cp.PaymentDate)
            .ToListAsync(cancellationToken);
    }
}
