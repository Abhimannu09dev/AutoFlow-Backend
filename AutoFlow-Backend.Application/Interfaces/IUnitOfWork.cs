namespace AutoFlow_Backend.Application.Interfaces;

public interface IUnitOfWork
{
    Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
