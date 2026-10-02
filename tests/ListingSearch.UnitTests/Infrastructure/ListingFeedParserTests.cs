using ListingSearch.Domain;
using ListingSearch.Infrastructure.Persistence;

namespace ListingSearch.UnitTests.Infrastructure;

public sealed class ListingFeedParserTests
{
    private const string ValidRecord = """
        { "id": "A1", "source": "MLS_A", "address": "1 Main St", "city": "Springfield", "state": "VA", "zip": "22150",
          "price": 450000, "bedrooms": 2, "bathrooms": 1.5, "sqft": 980, "latitude": 38.7, "longitude": -77.1,
          "listedDate": "2026-08-29", "status": "active", "description": "Bright condo." }
        """;

    private static string Record(string id = "A1", string source = "MLS_A", string price = "450000", string status = "\"active\"", string listedDate = "\"2026-08-29\"") => $$"""
        { "id": "{{id}}", "source": "{{source}}", "address": "1 Main St", "city": "Springfield", "state": "VA", "zip": "22150",
          "price": {{price}}, "bedrooms": 2, "bathrooms": 1.5, "sqft": 980, "latitude": 38.7, "longitude": -77.1,
          "listedDate": {{listedDate}}, "status": {{status}}, "description": "Bright condo." }
        """;

    private static FeedParseResult Parse(params string[] records) => ListingFeedParser.Parse($"[{string.Join(',', records)}]");

    [Fact]
    public void Valid_record_is_parsed()
    {
        var result = Parse(ValidRecord);

        var listing = Assert.Single(result.Listings);
        Assert.Empty(result.Rejected);
        Assert.Equal(new ListingKey("MLS_A", "A1"), listing.Key);
        Assert.Equal(450_000m, listing.Price);
        Assert.Equal(new DateOnly(2026, 8, 29), listing.ListedDate);
        Assert.Equal(ListingStatus.Active, listing.Status);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{ \"id\": \"A1\" }")]
    public void Unusable_document_is_fatal(string json)
    {
        Assert.Throws<ListingDataException>(() => ListingFeedParser.Parse(json));
    }

    [Fact]
    public void Bad_record_is_rejected_without_losing_the_others()
    {
        var result = Parse(Record(id: "good"), Record(id: "bad", price: "-5"));

        Assert.Equal("good", Assert.Single(result.Listings).Id);
        var rejected = Assert.Single(result.Rejected);
        Assert.Equal(1, rejected.Index);
        Assert.Equal("MLS_A:bad", rejected.Identifier);
    }

    [Theory]
    [InlineData("\"abc\"")]  // wrong type
    [InlineData("null")]     // missing required number
    public void Unusable_price_is_rejected(string price)
    {
        Assert.Single(Parse(Record(price: price)).Rejected);
    }

    [Fact]
    public void Unknown_status_is_rejected()
    {
        Assert.Single(Parse(Record(status: "\"withdrawn\"")).Rejected);
    }

    [Fact]
    public void Status_is_case_insensitive_and_trimmed()
    {
        Assert.Equal(ListingStatus.Pending, Assert.Single(Parse(Record(status: "\" PENDING \"")).Listings).Status);
    }

    [Theory]
    [InlineData("\"29/08/2026\"")]
    [InlineData("\"\"")]
    [InlineData("null")]
    public void Unparseable_date_is_rejected(string listedDate)
    {
        Assert.Single(Parse(Record(listedDate: listedDate)).Rejected);
    }

    [Fact]
    public void Iso_date_time_is_reduced_to_its_utc_date()
    {
        var listing = Assert.Single(Parse(Record(listedDate: "\"2026-08-29T23:30:00-05:00\"")).Listings);

        Assert.Equal(new DateOnly(2026, 8, 30), listing.ListedDate);
    }

    [Fact]
    public void Duplicate_source_and_id_keeps_the_first()
    {
        var result = Parse(Record(price: "100"), Record(price: "200"));

        Assert.Equal(100m, Assert.Single(result.Listings).Price);
        Assert.Single(result.Rejected);
    }

    [Fact]
    public void Same_id_from_different_sources_is_kept()
    {
        Assert.Equal(2, Parse(Record(source: "MLS_A"), Record(source: "MLS_B")).Listings.Count);
    }

    [Fact]
    public void Null_element_is_rejected()
    {
        var result = Parse(ValidRecord, "null");

        Assert.Single(result.Listings);
        Assert.Equal("#1", Assert.Single(result.Rejected).Identifier);
    }
}
