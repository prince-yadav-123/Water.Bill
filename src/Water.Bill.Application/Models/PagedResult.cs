using Water.Bill.Core.Common;

namespace Water.Bill.Application.Models;

/// <summary>Generic paged result containing items and metadata for a single page.</summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public int From => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
    public int To => Math.Min(Page * PageSize, TotalCount);
    public bool HasPrev => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static PagedResult<T> Empty(int page, int pageSize)
        => new() { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };
}
