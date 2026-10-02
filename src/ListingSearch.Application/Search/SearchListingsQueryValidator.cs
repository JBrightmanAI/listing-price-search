using ListingSearch.Application.Common;

namespace ListingSearch.Application.Search;

/// <summary>
/// All input rules for a search in one place. Every rule is checked so the caller sees every problem at once.
/// A valid query that matches nothing is not an error; that is decided by the search, not here.
/// </summary>
public static class SearchListingsQueryValidator
{
    public static ValidationErrors Validate(SearchListingsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var errors = new ValidationErrors();

        if (query.MinPrice < 0)
        {
            errors.Add(SearchFields.MinPrice, "minPrice cannot be negative.");
        }

        if (query.MaxPrice < 0)
        {
            errors.Add(SearchFields.MaxPrice, "maxPrice cannot be negative.");
        }

        if (query.MinPrice is { } min && query.MaxPrice is { } max && min > max)
        {
            errors.Add(SearchFields.MinPrice, "minPrice cannot be greater than maxPrice.");
        }

        if (query.MinBedrooms < 0)
        {
            errors.Add(SearchFields.MinBedrooms, "minBedrooms cannot be negative.");
        }

        if (query.TargetBudget <= 0)
        {
            errors.Add(SearchFields.TargetBudget, "targetBudget must be greater than zero.");
        }

        var city = SearchText.NormalizeWhitespace(query.City);
        if (city?.Length > SearchLimits.MaxCityLength)
        {
            errors.Add(SearchFields.City, $"city cannot be longer than {SearchLimits.MaxCityLength} characters.");
        }

        var keyword = SearchText.NormalizeWhitespace(query.Keyword);
        if (keyword is not null)
        {
            if (keyword.Length > SearchLimits.MaxKeywordLength)
            {
                errors.Add(SearchFields.Keyword, $"keyword cannot be longer than {SearchLimits.MaxKeywordLength} characters.");
            }
            else if (SearchText.Tokenize(keyword).Count == 0)
            {
                // Otherwise "!!!" would silently behave like no keyword and return everything.
                errors.Add(SearchFields.Keyword, "keyword must contain at least one letter or digit.");
            }
        }

        if (query.Page < 1)
        {
            errors.Add(SearchFields.Page, "page must be 1 or greater.");
        }

        if (query.PageSize is < 1 or > SearchLimits.MaxPageSize)
        {
            errors.Add(SearchFields.PageSize, $"pageSize must be between 1 and {SearchLimits.MaxPageSize}.");
        }

        return errors;
    }
}
