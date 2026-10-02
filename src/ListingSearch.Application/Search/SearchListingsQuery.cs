namespace ListingSearch.Application.Search;

/// <summary>Input to the listing search use case. Every filter is optional; null means "not applied".</summary>
public sealed record SearchListingsQuery
{
    public decimal? MinPrice { get; init; }

    public decimal? MaxPrice { get; init; }

    public int? MinBedrooms { get; init; }

    public string? City { get; init; }

    public string? Keyword { get; init; }

    public decimal? TargetBudget { get; init; }

    public int Page { get; init; } = SearchLimits.DefaultPage;

    public int PageSize { get; init; } = SearchLimits.DefaultPageSize;
}

public static class SearchLimits
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;
    public const int MaxCityLength = 100;
    public const int MaxKeywordLength = 100;
}

/// <summary>
/// Field names used in validation errors. They match the public query parameter names
/// so a client can show each message next to the input that caused it.
/// </summary>
public static class SearchFields
{
    public const string MinPrice = "minPrice";
    public const string MaxPrice = "maxPrice";
    public const string MinBedrooms = "minBedrooms";
    public const string City = "city";
    public const string Keyword = "keyword";
    public const string TargetBudget = "targetBudget";
    public const string Page = "page";
    public const string PageSize = "pageSize";
}
