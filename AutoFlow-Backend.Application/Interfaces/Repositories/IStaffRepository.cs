using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IStaffRepository : IRepositoryBase<Staff>
{
    Task<PagedResponse<Staff>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string normalizedEmail, Guid? excludeStaffId = null, CancellationToken cancellationToken = default);
    Task<bool> StaffCodeExistsAsync(string normalizedStaffCode, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Staff?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Staff?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
    Task<Staff?> GetActiveByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
}
