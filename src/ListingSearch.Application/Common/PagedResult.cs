namespace ListingSearch.Application.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    /// <summary>Zero when there are no results. Computed in 64-bit so large page sizes cannot overflow.</summary>
    public int TotalPages => TotalCount == 0
        ? 0
        : (int)((TotalCount + (long)PageSize - 1) / PageSize);
}
