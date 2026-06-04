using Microsoft.EntityFrameworkCore;
using Water.Bill.Application.Models;
using Water.Bill.Core.Common;

namespace Water.Bill.Infrastructure.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Executes a COUNT and a paginated SELECT against the database using EF Core.
    /// Paging happens at database level — no in-memory buffering.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = PagingConstants.ValidatePage(page);
        pageSize = PagingConstants.Validate(pageSize);

        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0)
            return PagedResult<T>.Empty(page, pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
