using AutoFlow_Backend.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Infrastructure.Common;

internal static class PaginationHelper
{
    public static async Task<PagedResponse<T>> ToPagedAsync<T>(
        IQueryable<T> source,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
