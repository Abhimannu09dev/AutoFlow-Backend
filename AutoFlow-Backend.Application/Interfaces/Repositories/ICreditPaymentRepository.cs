using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface ICreditPaymentRepository : IRepositoryBase<CreditPayment>
{
    Task<List<CreditPayment>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);
}
