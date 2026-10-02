using ListingSearch.Application.Search;
using Microsoft.AspNetCore.Mvc;

namespace ListingSearch.Api.Contracts;

/// <summary>
/// Query-string contract for GET /api/listings/search. Explicit names keep the public contract
/// stable if C# property names change, and make validation error keys match the query parameters.
/// </summary>
public sealed class SearchListingsRequest
{
    [FromQuery(Name = SearchFields.MinPrice)]
    public decimal? MinPrice { get; init; }

    [FromQuery(Name = SearchFields.MaxPrice)]
    public decimal? MaxPrice { get; init; }

    [FromQuery(Name = SearchFields.MinBedrooms)]
    public int? MinBedrooms { get; init; }

    [FromQuery(Name = SearchFields.City)]
    public string? City { get; init; }

    [FromQuery(Name = SearchFields.Keyword)]
    public string? Keyword { get; init; }

    [FromQuery(Name = SearchFields.TargetBudget)]
    public decimal? TargetBudget { get; init; }

    [FromQuery(Name = SearchFields.Page)]
    public int? Page { get; init; }

    [FromQuery(Name = SearchFields.PageSize)]
    public int? PageSize { get; init; }

    public SearchListingsQuery ToQuery() => new()
    {
        MinPrice = MinPrice,
        MaxPrice = MaxPrice,
        MinBedrooms = MinBedrooms,
        City = City,
        Keyword = Keyword,
        TargetBudget = TargetBudget,
        Page = Page ?? SearchLimits.DefaultPage,
        PageSize = PageSize ?? SearchLimits.DefaultPageSize,
    };
}
