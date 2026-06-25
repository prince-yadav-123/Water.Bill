using Water.Bill.Application.Models;
using Water.Bill.Core.Common;

namespace Water.Bill.API.Models;

/// <summary>
/// View-model passed to the _Pagination partial view.
/// Build one from a PagedResult using the static factory.
/// </summary>
public sealed class PaginationViewModel
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public int From { get; init; }
    public int To { get; init; }
    public bool HasPrev { get; init; }
    public bool HasNext { get; init; }
    public int[] AllowedPageSizes { get; init; } = PagingConstants.AllowedPageSizes;

    public static PaginationViewModel Create<T>(PagedResult<T> result)
        => new()
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages,
            From = result.From,
            To = result.To,
            HasPrev = result.HasPrev,
            HasNext = result.HasNext
        };
}
