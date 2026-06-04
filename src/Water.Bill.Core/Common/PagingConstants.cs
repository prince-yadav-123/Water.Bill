namespace Water.Bill.Core.Common;

public static class PagingConstants
{
    public const int DefaultPageSize = 10;
    public static readonly int[] AllowedPageSizes = [10, 50, 100, 1000];

    /// <summary>Returns the page size if it is in the allowed list, otherwise DefaultPageSize.</summary>
    public static int Validate(int pageSize)
        => AllowedPageSizes.Contains(pageSize) ? pageSize : DefaultPageSize;

    /// <summary>Returns a valid page number (≥1).</summary>
    public static int ValidatePage(int page)
        => page < 1 ? 1 : page;
}
