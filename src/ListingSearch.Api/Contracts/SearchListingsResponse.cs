using ListingSearch.Application.Common;
using ListingSearch.Application.Search;
using ListingSearch.Domain;

namespace ListingSearch.Api.Contracts;

public sealed record SearchListingsResponse(
    IReadOnlyList<ListingResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public static SearchListingsResponse From(PagedResult<ScoredListing> page)
    {
        ArgumentNullException.ThrowIfNull(page);

        return new SearchListingsResponse(
            page.Items.Select(ListingResponse.From).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount,
            page.TotalPages);
    }
}

/// <summary>The public shape of a listing. Kept separate from the domain type so either can change independently.</summary>
public sealed record ListingResponse(
    string Id,
    string Source,
    string Address,
    string City,
    string State,
    string Zip,
    decimal Price,
    int Bedrooms,
    decimal Bathrooms,
    int Sqft,
    double Latitude,
    double Longitude,
    DateOnly ListedDate,
    string Status,
    string Description,
    double RelevanceScore)
{
    public static ListingResponse From(ScoredListing scored)
    {
        ArgumentNullException.ThrowIfNull(scored);
        var listing = scored.Listing;

        return new ListingResponse(
            listing.Id,
            listing.Source,
            listing.Address,
            listing.City,
            listing.State,
            listing.Zip,
            listing.Price,
            listing.Bedrooms,
            listing.Bathrooms,
            listing.Sqft,
            listing.Latitude,
            listing.Longitude,
            listing.ListedDate,
            ToContract(listing.Status),
            listing.Description,
            scored.Score);
    }

    private static string ToContract(ListingStatus status) => status switch
    {
        ListingStatus.Active => "active",
        ListingStatus.Pending => "pending",
        ListingStatus.Sold => "sold",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown listing status."),
    };
}
