using System.Net;
using System.Text.Json;

namespace ListingSearch.IntegrationTests;

public sealed class SearchEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string SearchUrl = "/api/listings/search";

    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(HttpResponseMessage Response, JsonElement Body)> GetAsync(string query)
    {
        var response = await _client.GetAsync(new Uri($"{SearchUrl}{query}", UriKind.Relative));
        var json = await response.Content.ReadAsStringAsync();
        return (response, JsonDocument.Parse(json).RootElement.Clone());
    }

    [Fact]
    public async Task Returns_a_ranked_page_with_the_documented_shape()
    {
        var (response, body) = await GetAsync("?targetBudget=450000&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, body.GetProperty("page").GetInt32());
        Assert.Equal(5, body.GetProperty("pageSize").GetInt32());
        Assert.Equal(12, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, body.GetProperty("totalPages").GetInt32());

        var items = body.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(5, items.Count);

        var first = items[0];
        foreach (var field in new[] { "id", "source", "address", "city", "price", "bedrooms", "listedDate", "status", "relevanceScore" })
        {
            Assert.True(first.TryGetProperty(field, out _), $"Missing field '{field}'.");
        }

        var scores = items.Select(i => i.GetProperty("relevanceScore").GetDouble()).ToList();
        Assert.Equal(scores.OrderDescending(), scores);
    }

    [Fact]
    public async Task Ranking_on_sample_data_prefers_recent_listings_near_budget()
    {
        // A5 (470k, listed yesterday) edges out A1 (exactly 450k, listed 7 days ago): recency outweighs a 4% price gap.
        var (_, body) = await GetAsync("?targetBudget=450000&pageSize=2");

        var ids = body.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetString()).ToList();
        Assert.Equal(["A5", "A1"], ids);
    }

    [Fact]
    public async Task Ids_shared_across_sources_are_all_returned()
    {
        var (_, body) = await GetAsync("?pageSize=100");

        var keys = body.GetProperty("items").EnumerateArray()
            .Select(i => $"{i.GetProperty("source").GetString()}:{i.GetProperty("id").GetString()}")
            .ToList();

        Assert.Equal(12, keys.Distinct().Count());
    }

    [Fact]
    public async Task Filters_are_applied()
    {
        var (_, body) = await GetAsync("?city=reston&minBedrooms=4");

        var item = Assert.Single(body.GetProperty("items").EnumerateArray());
        Assert.Equal("A4", item.GetProperty("id").GetString());
    }

    [Fact]
    public async Task No_matches_is_200_with_an_empty_page()
    {
        var (response, body) = await GetAsync("?city=Atlantis");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(body.GetProperty("items").EnumerateArray());
        Assert.Equal(0, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, body.GetProperty("totalPages").GetInt32());
    }

    [Fact]
    public async Task Page_beyond_the_last_is_empty_with_correct_totals()
    {
        var (response, body) = await GetAsync("?page=99&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(body.GetProperty("items").EnumerateArray());
        Assert.Equal(3, body.GetProperty("totalPages").GetInt32());
    }

    [Fact]
    public async Task Empty_query_values_are_treated_as_not_supplied()
    {
        var (response, body) = await GetAsync("?minPrice=&city=&keyword=");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(12, body.GetProperty("totalCount").GetInt32());
    }

    [Theory]
    [InlineData("?minPrice=500000&maxPrice=400000", "minPrice")]
    [InlineData("?pageSize=0", "pageSize")]
    [InlineData("?pageSize=-1", "pageSize")]
    [InlineData("?pageSize=101", "pageSize")]
    [InlineData("?page=0", "page")]
    [InlineData("?targetBudget=0", "targetBudget")]
    [InlineData("?keyword=%21%21%21", "keyword")]
    [InlineData("?minPrice=abc", "minPrice")]
    [InlineData("?page=99999999999", "page")]
    [InlineData("?minBedrooms=2.5", "minBedrooms")]
    public async Task Invalid_input_is_a_400_problem_keyed_by_parameter(string query, string field)
    {
        var (response, body) = await GetAsync(query);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(400, body.GetProperty("status").GetInt32());
        Assert.True(body.GetProperty("errors").TryGetProperty(field, out _), $"Expected an error for '{field}'.");
        Assert.True(body.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task Several_invalid_parameters_are_reported_together()
    {
        var (_, body) = await GetAsync("?minPrice=10&maxPrice=5&pageSize=0");

        var errors = body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("minPrice", out _));
        Assert.True(errors.TryGetProperty("pageSize", out _));
    }
}
