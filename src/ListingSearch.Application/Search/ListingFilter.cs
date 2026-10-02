using ListingSearch.Domain;

namespace ListingSearch.Application.Search;

/// <summary>
/// The filter part of a search, built once per request from an already validated query.
/// All conditions must hold; a filter that was not supplied always passes.
/// </summary>
internal sealed class ListingFilter
{
    private readonly decimal? _minPrice;
    private readonly decimal? _maxPrice;
    private readonly int? _minBedrooms;
    private readonly string? _city;
    private readonly IReadOnlyList<string> _keywordTerms;

    private ListingFilter(decimal? minPrice, decimal? maxPrice, int? minBedrooms, string? city, IReadOnlyList<string> keywordTerms)
    {
        _minPrice = minPrice;
        _maxPrice = maxPrice;
        _minBedrooms = minBedrooms;
        _city = city;
        _keywordTerms = keywordTerms;
    }

    public static ListingFilter From(SearchListingsQuery query) => new(
        query.MinPrice,
        query.MaxPrice,
        query.MinBedrooms,
        SearchText.NormalizeWhitespace(query.City),
        SearchText.Tokenize(query.Keyword).Distinct(StringComparer.Ordinal).ToArray());

    public bool Matches(Listing listing) =>
        (_minPrice is null || listing.Price >= _minPrice)
        && (_maxPrice is null || listing.Price <= _maxPrice)
        && (_minBedrooms is null || listing.Bedrooms >= _minBedrooms)
        && MatchesCity(listing)
        && MatchesKeyword(listing);

    /// <summary>Exact city match, ignoring case and whitespace differences between feeds.</summary>
    private bool MatchesCity(Listing listing) =>
        _city is null
        || string.Equals(SearchText.NormalizeWhitespace(listing.City), _city, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Every keyword term must start a word in the description, so "walk" matches "walkable"
    /// but "pet" does not match "carpet". This is plain text matching: it cannot tell
    /// "pets allowed" from "no pets"; see the README for that limitation.
    /// </summary>
    private bool MatchesKeyword(Listing listing)
    {
        if (_keywordTerms.Count == 0)
        {
            return true;
        }

        var words = SearchText.Tokenize(listing.Description);
        return _keywordTerms.All(term => words.Any(word => word.StartsWith(term, StringComparison.Ordinal)));
    }
}
