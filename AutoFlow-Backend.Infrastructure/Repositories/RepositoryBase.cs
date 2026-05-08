using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Infrastructure.Data;

namespace AutoFlow_Backend.Infrastructure.Repositories;

public class RepositoryBase<T>(AppDbContext context) : IRepositoryBase<T>
    where T : class
{
    protected readonly AppDbContext Context = context;

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        Context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        Context.Set<T>().Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}