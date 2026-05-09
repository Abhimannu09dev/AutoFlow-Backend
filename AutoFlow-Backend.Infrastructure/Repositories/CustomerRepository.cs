using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Repositories;

public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }

    public async Task<bool> EmailExistsAsync(
        string normalizedEmail,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default) =>
        await Context.Set<Customer>()
            .AsNoTracking()
            .AnyAsync(c => c.Email == normalizedEmail && (excludeId == null || c.Id != excludeId), cancellationToken);

    public async Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.Set<Customer>()
            .AsNoTracking()
            .OrderBy(c => c.FullName)
            .ToListAsync(cancellationToken);

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Set<Customer>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Customer?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Set<Customer>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<List<Customer>> SearchAsync(
        string normalizedLowerQuery,
        IReadOnlyList<Guid> matchingUserIds,
        Guid? customerIdMatch,
        CancellationToken cancellationToken = default) =>
        await Context.Set<Customer>()
            .AsNoTracking()
            .Where(c =>
                c.FullName.ToLower().Contains(normalizedLowerQuery) ||
                c.Email.ToLower().Contains(normalizedLowerQuery) ||
                (c.Phone != null && c.Phone.ToLower().Contains(normalizedLowerQuery)) ||
                (customerIdMatch.HasValue && c.Id == customerIdMatch.Value) ||
                (c.ApplicationUserId.HasValue && matchingUserIds.Contains(c.ApplicationUserId.Value)))
            .OrderBy(c => c.FullName)
            .ToListAsync(cancellationToken);

    public async Task<Customer?> GetByApplicationUserIdAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken = default) =>
        await Context.Set<Customer>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId, cancellationToken);

    public async Task<Customer?> GetByApplicationUserIdForUpdateAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken = default) =>
        await Context.Set<Customer>()
            .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId, cancellationToken);
}