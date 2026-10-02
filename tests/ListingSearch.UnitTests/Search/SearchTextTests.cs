using ListingSearch.Application.Search;

namespace ListingSearch.UnitTests.Search;

public sealed class SearchTextTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData("  New   York ", "New York")]
    [InlineData("Reston", "Reston")]
    public void NormalizeWhitespace_trims_and_collapses(string? input, string? expected)
    {
        Assert.Equal(expected, SearchText.NormalizeWhitespace(input));
    }

    [Fact]
    public void Tokenize_splits_on_punctuation_and_lower_cases()
    {
        Assert.Equal(["top", "floor", "condo", "2", "car"], SearchText.Tokenize("Top-floor CONDO, 2-car."));
    }

    [Fact]
    public void Tokenize_returns_nothing_for_punctuation_only()
    {
        Assert.Empty(SearchText.Tokenize("!!! ---"));
    }
}
