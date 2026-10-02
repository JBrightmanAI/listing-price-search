using ListingSearch.Application.Search;
using ListingSearch.UnitTests.TestSupport;

namespace ListingSearch.UnitTests.Search;

public sealed class ListingRankComparerTests
{
    private static string[] Rank(params ScoredListing[] listings) =>
        listings.Order(ListingRankComparer.Instance).Select(s => s.Listing.Key.ToString()).ToArray();

    [Fact]
    public void Higher_score_ranks_first()
    {
        var low = new ScoredListing(new ListingBuilder().WithId("low").Build(), 10);
        var high = new ScoredListing(new ListingBuilder().WithId("high").Build(), 90);

        Assert.Equal(["MLS_A:high", "MLS_A:low"], Rank(low, high));
    }

    [Fact]
    public void Tie_is_broken_by_newer_listing_first()
    {
        var older = new ScoredListing(new ListingBuilder().WithId("older").ListedDaysAgo(10).Build(), 50);
        var newer = new ScoredListing(new ListingBuilder().WithId("newer").ListedDaysAgo(1).Build(), 50);

        Assert.Equal(["MLS_A:newer", "MLS_A:older"], Rank(older, newer));
    }

    [Fact]
    public void Then_by_lower_price()
    {
        var pricier = new ScoredListing(new ListingBuilder().WithId("pricier").WithPrice(600_000m).Build(), 50);
        var cheaper = new ScoredListing(new ListingBuilder().WithId("cheaper").WithPrice(400_000m).Build(), 50);

        Assert.Equal(["MLS_A:cheaper", "MLS_A:pricier"], Rank(pricier, cheaper));
    }

    [Fact]
    public void Then_by_source_then_id()
    {
        var b1 = new ScoredListing(new ListingBuilder().WithKey("MLS_B", "1").Build(), 50);
        var a2 = new ScoredListing(new ListingBuilder().WithKey("MLS_A", "2").Build(), 50);
        var a1 = new ScoredListing(new ListingBuilder().WithKey("MLS_A", "1").Build(), 50);

        Assert.Equal(["MLS_A:1", "MLS_A:2", "MLS_B:1"], Rank(b1, a2, a1));
    }
}
