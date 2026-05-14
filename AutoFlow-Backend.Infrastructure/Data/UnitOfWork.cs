using AutoFlow_Backend.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoFlow_Backend.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfCoreAppTransaction(transaction);
    }

    private sealed class EfCoreAppTransaction : IAppTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfCoreAppTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            _transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default) =>
            _transaction.RollbackAsync(cancellationToken);

        public ValueTask DisposeAsync() =>
            _transaction.DisposeAsync();
    }
}
