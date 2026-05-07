using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Background.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Background.Infrastructure.Repositories;

public class StaffRepository(AppDbContext context)
    : RepositoryBase<Staff>(context), IStaffRepository
{
    public Task<bool> EmailExistsAsync(
        string normalizedEmail,
        Guid? excludeStaffId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Staff
            .AsNoTracking()
            .Where(staff => staff.Email.ToLower() == normalizedEmail);

        if (excludeStaffId.HasValue)
        {
            query = query.Where(staff => staff.Id != excludeStaffId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> StaffCodeExistsAsync(string normalizedStaffCode, CancellationToken cancellationToken = default)
    {
        return Context.Staff
            .AsNoTracking()
            .AnyAsync(staff => staff.StaffCode == normalizedStaffCode, cancellationToken);
    }

    public Task<List<Staff>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Context.Staff
            .AsNoTracking()
            .OrderBy(staff => staff.FirstName)
            .ThenBy(staff => staff.LastName)
            .ToListAsync(cancellationToken);
    }

    public Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Staff
            .AsNoTracking()
            .FirstOrDefaultAsync(staff => staff.Id == id, cancellationToken);
    }

    public Task<Staff?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Staff
            .FirstOrDefaultAsync(staff => staff.Id == id, cancellationToken);
    }

    public Task<Staff?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default)
    {
        return Context.Staff
            .AsNoTracking()
            .FirstOrDefaultAsync(staff => staff.ApplicationUserId == applicationUserId, cancellationToken);
    }
}
