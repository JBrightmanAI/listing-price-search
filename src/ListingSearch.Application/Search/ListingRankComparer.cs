namespace ListingSearch.Application.Search;

/// <summary>
/// Orders results by score, then breaks ties so the order is total and stable across requests and pages:
/// newer listing first, then lower price, then source, then id.
/// </summary>
internal sealed class ListingRankComparer : IComparer<ScoredListing>
{
    public static readonly ListingRankComparer Instance = new();

    public int Compare(ScoredListing? x, ScoredListing? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return 1;
        }

        if (y is null)
        {
            return -1;
        }

        var result = y.Score.CompareTo(x.Score);
        if (result != 0)
        {
            return result;
        }

        result = y.Listing.ListedDate.CompareTo(x.Listing.ListedDate);
        if (result != 0)
        {
            return result;
        }

        result = x.Listing.Price.CompareTo(y.Listing.Price);
        if (result != 0)
        {
            return result;
        }

        result = string.CompareOrdinal(x.Listing.Source, y.Listing.Source);
        return result != 0 ? result : string.CompareOrdinal(x.Listing.Id, y.Listing.Id);
    }
}
