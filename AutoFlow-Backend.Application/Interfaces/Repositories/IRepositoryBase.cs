namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IRepositoryBase<T> where T : class
{
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
