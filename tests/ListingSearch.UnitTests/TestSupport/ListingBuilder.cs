using ListingSearch.Domain;

namespace ListingSearch.UnitTests.TestSupport;

/// <summary>Builds valid listings with sensible defaults so each test states only what it cares about.</summary>
internal sealed class ListingBuilder
{
    private string _source = "MLS_A";
    private string _id = "1";
    private string _city = "Springfield";
    private string _description = "A nice home.";
    private decimal _price = 500_000m;
    private int _bedrooms = 3;
    private DateOnly _listedDate = TestClock.Today;
    private ListingStatus _status = ListingStatus.Active;

    public ListingBuilder WithKey(string source, string id)
    {
        _source = source;
        _id = id;
        return this;
    }

    public ListingBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ListingBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public ListingBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ListingBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public ListingBuilder WithBedrooms(int bedrooms)
    {
        _bedrooms = bedrooms;
        return this;
    }

    public ListingBuilder ListedDaysAgo(int days)
    {
        _listedDate = TestClock.Today.AddDays(-days);
        return this;
    }

    public ListingBuilder WithStatus(ListingStatus status)
    {
        _status = status;
        return this;
    }

    public Listing Build() => Listing.Create(
        _source,
        _id,
        $"{_id} Test St",
        _city,
        "VA",
        "22150",
        _price,
        _bedrooms,
        2m,
        1500,
        38.8,
        -77.2,
        _listedDate,
        _status,
        _description);
}
