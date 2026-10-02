using ListingSearch.Domain;

namespace ListingSearch.UnitTests.Domain;

public sealed class ListingTests
{
    private static Listing Create(
        string? source = "MLS_A",
        string? id = "A1",
        string? address = "1 Main St",
        string? city = "Springfield",
        decimal price = 100_000m,
        int bedrooms = 2,
        decimal bathrooms = 1m,
        int sqft = 800,
        double latitude = 38.8,
        double longitude = -77.2,
        ListingStatus status = ListingStatus.Active,
        string? description = "desc") =>
        Listing.Create(source, id, address, city, "VA", "22150", price, bedrooms, bathrooms, sqft, latitude, longitude, new DateOnly(2026, 9, 1), status, description);

    [Fact]
    public void Create_trims_text_fields()
    {
        var listing = Listing.Create(" MLS_A ", " A1 ", " 1 Main St ", " Springfield ", " VA ", " 22150 ", 1m, 0, 0m, 0, 0, 0, new DateOnly(2026, 9, 1), ListingStatus.Active, "  text  ");

        Assert.Equal(new ListingKey("MLS_A", "A1"), listing.Key);
        Assert.Equal("1 Main St", listing.Address);
        Assert.Equal("Springfield", listing.City);
        Assert.Equal("text", listing.Description);
    }

    [Fact]
    public void Create_allows_missing_description()
    {
        Assert.Equal(string.Empty, Create(description: null).Description);
    }

    [Theory]
    [InlineData(null, "A1", "1 Main St", "Springfield")]
    [InlineData("MLS_A", " ", "1 Main St", "Springfield")]
    [InlineData("MLS_A", "A1", "", "Springfield")]
    [InlineData("MLS_A", "A1", "1 Main St", null)]
    public void Create_rejects_missing_required_text(string? source, string? id, string? address, string? city)
    {
        Assert.Throws<DomainValidationException>(() => Create(source, id, address, city));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_non_positive_price(decimal price)
    {
        Assert.Throws<DomainValidationException>(() => Create(price: price));
    }

    [Fact]
    public void Create_rejects_negative_counts()
    {
        Assert.Throws<DomainValidationException>(() => Create(bedrooms: -1));
        Assert.Throws<DomainValidationException>(() => Create(bathrooms: -0.5m));
        Assert.Throws<DomainValidationException>(() => Create(sqft: -1));
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(90.1, 0)]
    [InlineData(-90.1, 0)]
    [InlineData(0, 180.1)]
    [InlineData(0, double.NaN)]
    public void Create_rejects_invalid_coordinates(double latitude, double longitude)
    {
        Assert.Throws<DomainValidationException>(() => Create(latitude: latitude, longitude: longitude));
    }

    [Fact]
    public void Create_rejects_undefined_status()
    {
        Assert.Throws<DomainValidationException>(() => Create(status: (ListingStatus)42));
    }

    [Fact]
    public void Same_id_from_different_sources_are_different_listings()
    {
        Assert.NotEqual(Create(source: "MLS_A", id: "1").Key, Create(source: "MLS_B", id: "1").Key);
    }
}
