using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Infrastructure.Common;
using AutoFlow_Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Repositories;

public class AppointmentRepository : RepositoryBase<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext context) : base(context) { }

    public Task<PagedResponse<Appointment>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Appointment>()
            .AsNoTracking()
            .Include(a => a.Vehicle)
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Time);

        return PaginationHelper.ToPagedAsync(query, request, cancellationToken);
    }

    public Task<PagedResponse<Appointment>> GetPagedByCustomerIdAsync(Guid customerId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Appointment>()
            .AsNoTracking()
            .Include(a => a.Vehicle)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Time);

        return PaginationHelper.ToPagedAsync(query, request, cancellationToken);
    }

    public async Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Context.Set<Appointment>()
            .AsNoTracking()
            .Include(a => a.Vehicle)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Time)
            .ToListAsync(cancellationToken);

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Set<Appointment>()
            .AsNoTracking()
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<List<Appointment>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default) =>
        await Context.Set<Appointment>()
            .AsNoTracking()
            .Include(a => a.Vehicle)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.Time)
            .ToListAsync(cancellationToken);
}