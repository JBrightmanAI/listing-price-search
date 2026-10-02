using ListingSearch.Application.Search;

namespace ListingSearch.UnitTests.Search;

public sealed class SearchListingsQueryValidatorTests
{
    private static IReadOnlyDictionary<string, string[]> Validate(SearchListingsQuery query) =>
        SearchListingsQueryValidator.Validate(query).ToDictionary();

    [Fact]
    public void Empty_query_is_valid()
    {
        Assert.Empty(Validate(new SearchListingsQuery()));
    }

    [Fact]
    public void MinPrice_greater_than_maxPrice_is_rejected_on_minPrice()
    {
        var errors = Validate(new SearchListingsQuery { MinPrice = 600_000m, MaxPrice = 500_000m });

        Assert.Equal(["minPrice cannot be greater than maxPrice."], errors[SearchFields.MinPrice]);
    }

    [Fact]
    public void MinPrice_equal_to_maxPrice_is_valid()
    {
        Assert.Empty(Validate(new SearchListingsQuery { MinPrice = 500_000m, MaxPrice = 500_000m }));
    }

    [Fact]
    public void Negative_values_are_rejected()
    {
        var errors = Validate(new SearchListingsQuery { MinPrice = -1m, MaxPrice = -1m, MinBedrooms = -1 });

        Assert.Contains(SearchFields.MinPrice, errors.Keys);
        Assert.Contains(SearchFields.MaxPrice, errors.Keys);
        Assert.Contains(SearchFields.MinBedrooms, errors.Keys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void TargetBudget_must_be_positive(decimal targetBudget)
    {
        Assert.Contains(SearchFields.TargetBudget, Validate(new SearchListingsQuery { TargetBudget = targetBudget }).Keys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(SearchLimits.MaxPageSize + 1)]
    public void PageSize_outside_range_is_rejected(int pageSize)
    {
        Assert.Contains(SearchFields.PageSize, Validate(new SearchListingsQuery { PageSize = pageSize }).Keys);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(SearchLimits.MaxPageSize)]
    public void PageSize_at_bounds_is_valid(int pageSize)
    {
        Assert.Empty(Validate(new SearchListingsQuery { PageSize = pageSize }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    public void Page_below_one_is_rejected(int page)
    {
        Assert.Contains(SearchFields.Page, Validate(new SearchListingsQuery { Page = page }).Keys);
    }

    [Fact]
    public void Keyword_too_long_is_rejected()
    {
        var keyword = new string('a', SearchLimits.MaxKeywordLength + 1);

        Assert.Contains(SearchFields.Keyword, Validate(new SearchListingsQuery { Keyword = keyword }).Keys);
    }

    [Fact]
    public void Keyword_with_no_letters_or_digits_is_rejected()
    {
        Assert.Contains(SearchFields.Keyword, Validate(new SearchListingsQuery { Keyword = "!!! ---" }).Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_keyword_and_city_mean_no_filter(string blank)
    {
        Assert.Empty(Validate(new SearchListingsQuery { Keyword = blank, City = blank }));
    }

    [Fact]
    public void City_too_long_is_rejected()
    {
        var city = new string('a', SearchLimits.MaxCityLength + 1);

        Assert.Contains(SearchFields.City, Validate(new SearchListingsQuery { City = city }).Keys);
    }

    [Fact]
    public void All_errors_are_reported_together()
    {
        var errors = Validate(new SearchListingsQuery { MinPrice = 10m, MaxPrice = 5m, Page = 0, PageSize = 0 });

        Assert.Equal(
            new[] { SearchFields.MinPrice, SearchFields.Page, SearchFields.PageSize }.Order(),
            errors.Keys.Order());
    }
}
