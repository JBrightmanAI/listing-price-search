using ListingSearch.Application.Common;
using ListingSearch.Application.Scoring;
using ListingSearch.Application.Search;
using ListingSearch.Domain;
using ListingSearch.Infrastructure.Persistence;
using ListingSearch.UnitTests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ListingSearch.UnitTests.Search;

public sealed class ListingSearchServiceTests
{
    private static ListingSearchService CreateService(params Listing[] listings) => new(
        new InMemoryListingRepository(listings),
        new BudgetRecencyScorer(Options.Create(new ScoringOptions())),
        TestClock.Create(),
        NullLogger<ListingSearchService>.Instance);

    private static async Task<PagedResult<ScoredListing>> SearchOk(ListingSearchService service, SearchListingsQuery query)
    {
        var result = await service.SearchAsync(query, CancellationToken.None);
        Assert.True(result.IsSuccess, "Expected a successful search.");
        return result.Value;
    }

    private static string[] Ids(PagedResult<ScoredListing> page) => page.Items.Select(i => i.Listing.Id).ToArray();

    private static Listing[] Numbered(int count) =>
        Enumerable.Range(1, count)
            .Select(i => new ListingBuilder().WithId(i.ToString("D2", System.Globalization.CultureInfo.InvariantCulture)).ListedDaysAgo(i).Build())
            .ToArray();

    // ---- Validation ----------------------------------------------------------------------

    [Fact]
    public async Task Invalid_query_returns_errors_instead_of_throwing()
    {
        var result = await CreateService(new ListingBuilder().Build())
            .SearchAsync(new SearchListingsQuery { MinPrice = 2m, MaxPrice = 1m }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(SearchFields.MinPrice, result.Errors.ToDictionary().Keys);
    }

    // ---- Filtering -----------------------------------------------------------------------

    [Fact]
    public async Task No_filters_returns_everything_ranked_by_score()
    {
        var page = await SearchOk(CreateService(Numbered(5)), new SearchListingsQuery());

        Assert.Equal(5, page.TotalCount);
        Assert.Equal(page.Items.Select(i => i.Score).OrderDescending(), page.Items.Select(i => i.Score));
    }

    [Fact]
    public async Task Price_bounds_are_inclusive()
    {
        var service = CreateService(
            new ListingBuilder().WithId("below").WithPrice(399_999m).Build(),
            new ListingBuilder().WithId("min").WithPrice(400_000m).Build(),
            new ListingBuilder().WithId("max").WithPrice(500_000m).Build(),
            new ListingBuilder().WithId("above").WithPrice(500_001m).Build());

        var page = await SearchOk(service, new SearchListingsQuery { MinPrice = 400_000m, MaxPrice = 500_000m });

        Assert.Equal(["max", "min"], Ids(page).Order());
    }

    [Fact]
    public async Task MinBedrooms_is_inclusive()
    {
        var service = CreateService(
            new ListingBuilder().WithId("two").WithBedrooms(2).Build(),
            new ListingBuilder().WithId("three").WithBedrooms(3).Build());

        var page = await SearchOk(service, new SearchListingsQuery { MinBedrooms = 3 });

        Assert.Equal(["three"], Ids(page));
    }

    [Theory]
    [InlineData("Springfield")]
    [InlineData("  springFIELD  ")]
    public async Task City_ignores_case_and_surrounding_whitespace(string city)
    {
        var service = CreateService(
            new ListingBuilder().WithId("in").WithCity("Springfield").Build(),
            new ListingBuilder().WithId("out").WithCity("Fairfax").Build());

        var page = await SearchOk(service, new SearchListingsQuery { City = city });

        Assert.Equal(["in"], Ids(page));
    }

    [Fact]
    public async Task City_is_an_exact_match_not_a_substring()
    {
        var service = CreateService(new ListingBuilder().WithCity("West Springfield").Build());

        var page = await SearchOk(service, new SearchListingsQuery { City = "Springfield" });

        Assert.Empty(page.Items);
    }

    [Fact]
    public async Task City_with_no_matches_is_a_successful_empty_result()
    {
        var page = await SearchOk(CreateService(Numbered(3)), new SearchListingsQuery { City = "Atlantis" });

        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalCount);
        Assert.Equal(0, page.TotalPages);
    }

    [Fact]
    public async Task Keyword_matches_word_prefixes_ignoring_case()
    {
        var service = CreateService(
            new ListingBuilder().WithId("walkable").WithDescription("Walkable to Metro.").Build(),
            new ListingBuilder().WithId("carpet").WithDescription("New carpet throughout.").Build());

        Assert.Equal(["walkable"], Ids(await SearchOk(service, new SearchListingsQuery { Keyword = "WALK" })));
        Assert.Empty(Ids(await SearchOk(service, new SearchListingsQuery { Keyword = "pet" })));
    }

    [Fact]
    public async Task Keyword_with_several_words_requires_all_of_them()
    {
        var service = CreateService(
            new ListingBuilder().WithId("both").WithDescription("Fenced yard, near schools.").Build(),
            new ListingBuilder().WithId("one").WithDescription("Fenced yard.").Build());

        var page = await SearchOk(service, new SearchListingsQuery { Keyword = "yard schools" });

        Assert.Equal(["both"], Ids(page));
    }

    [Fact]
    public async Task Filters_combine_with_and()
    {
        var service = CreateService(
            new ListingBuilder().WithId("match").WithCity("Reston").WithBedrooms(4).WithPrice(600_000m).WithDescription("Pool").Build(),
            new ListingBuilder().WithId("wrong-city").WithCity("Vienna").WithBedrooms(4).WithPrice(600_000m).WithDescription("Pool").Build(),
            new ListingBuilder().WithId("too-few-beds").WithCity("Reston").WithBedrooms(2).WithPrice(600_000m).WithDescription("Pool").Build(),
            new ListingBuilder().WithId("no-keyword").WithCity("Reston").WithBedrooms(4).WithPrice(600_000m).WithDescription("Garage").Build());

        var page = await SearchOk(service, new SearchListingsQuery
        {
            City = "reston",
            MinBedrooms = 3,
            MinPrice = 500_000m,
            MaxPrice = 700_000m,
            Keyword = "pool",
        });

        Assert.Equal(["match"], Ids(page));
    }

    [Fact]
    public async Task All_statuses_are_returned()
    {
        var service = CreateService(
            new ListingBuilder().WithId("a").WithStatus(ListingStatus.Active).Build(),
            new ListingBuilder().WithId("p").WithStatus(ListingStatus.Pending).Build(),
            new ListingBuilder().WithId("s").WithStatus(ListingStatus.Sold).Build());

        Assert.Equal(3, (await SearchOk(service, new SearchListingsQuery())).TotalCount);
    }

    // ---- Ranking -------------------------------------------------------------------------

    [Fact]
    public async Task Closer_to_budget_ranks_higher_when_equally_recent()
    {
        var service = CreateService(
            new ListingBuilder().WithId("far").WithPrice(700_000m).Build(),
            new ListingBuilder().WithId("near").WithPrice(510_000m).Build());

        var page = await SearchOk(service, new SearchListingsQuery { TargetBudget = 500_000m });

        Assert.Equal(["near", "far"], Ids(page));
    }

    [Fact]
    public async Task Tied_scores_are_ordered_deterministically()
    {
        // Same distance from budget on either side and same date: identical scores.
        var service = CreateService(
            new ListingBuilder().WithKey("MLS_B", "over").WithPrice(550_000m).Build(),
            new ListingBuilder().WithKey("MLS_B", "under").WithPrice(450_000m).Build(),
            new ListingBuilder().WithKey("MLS_A", "under").WithPrice(450_000m).Build());

        var page = await SearchOk(service, new SearchListingsQuery { TargetBudget = 500_000m });

        Assert.Single(page.Items.Select(i => i.Score).Distinct());
        Assert.Equal(
            ["MLS_A:under", "MLS_B:under", "MLS_B:over"],
            page.Items.Select(i => i.Listing.Key.ToString()));
    }

    // ---- Pagination ----------------------------------------------------------------------

    [Fact]
    public async Task First_page_returns_page_size_items()
    {
        var page = await SearchOk(CreateService(Numbered(12)), new SearchListingsQuery { Page = 1, PageSize = 5 });

        Assert.Equal(5, page.Items.Count);
        Assert.Equal(12, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
    }

    [Fact]
    public async Task Last_page_can_be_partial()
    {
        var page = await SearchOk(CreateService(Numbered(12)), new SearchListingsQuery { Page = 3, PageSize = 5 });

        Assert.Equal(["11", "12"], Ids(page));
    }

    [Fact]
    public async Task Page_beyond_the_last_is_empty_but_keeps_totals()
    {
        var page = await SearchOk(CreateService(Numbered(12)), new SearchListingsQuery { Page = 4, PageSize = 5 });

        Assert.Empty(page.Items);
        Assert.Equal(12, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
    }

    [Fact]
    public async Task Huge_page_number_does_not_overflow()
    {
        var page = await SearchOk(CreateService(Numbered(3)), new SearchListingsQuery { Page = int.MaxValue, PageSize = SearchLimits.MaxPageSize });

        Assert.Empty(page.Items);
        Assert.Equal(1, page.TotalPages);
    }

    [Theory]
    [InlineData(12, 1)]
    [InlineData(100, 1)]
    [InlineData(1, 12)]
    public async Task Page_size_relative_to_total_gives_expected_page_count(int pageSize, int expectedPages)
    {
        var page = await SearchOk(CreateService(Numbered(12)), new SearchListingsQuery { PageSize = pageSize });

        Assert.Equal(expectedPages, page.TotalPages);
    }

    [Fact]
    public async Task Pages_do_not_overlap_and_cover_every_result()
    {
        var service = CreateService(Numbered(12));

        var all = new List<string>();
        for (var p = 1; p <= 3; p++)
        {
            all.AddRange(Ids(await SearchOk(service, new SearchListingsQuery { Page = p, PageSize = 5 })));
        }

        Assert.Equal(Ids(await SearchOk(service, new SearchListingsQuery { PageSize = 100 })), all);
    }

    // ---- Cancellation --------------------------------------------------------------------

    [Fact]
    public async Task Cancelled_request_stops_the_search()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CreateService(Numbered(3)).SearchAsync(new SearchListingsQuery(), cts.Token));
    }
}
