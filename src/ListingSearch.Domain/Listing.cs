namespace ListingSearch.Domain;

/// <summary>
/// A property listing as received from one MLS feed.
/// Instances can only be created through <see cref="Create"/>, so every listing in the system satisfies its invariants.
/// </summary>
public sealed class Listing
{
    private Listing(
        ListingKey key,
        string address,
        string city,
        string state,
        string zip,
        decimal price,
        int bedrooms,
        decimal bathrooms,
        int sqft,
        double latitude,
        double longitude,
        DateOnly listedDate,
        ListingStatus status,
        string description)
    {
        Key = key;
        Address = address;
        City = city;
        State = state;
        Zip = zip;
        Price = price;
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;
        Sqft = sqft;
        Latitude = latitude;
        Longitude = longitude;
        ListedDate = listedDate;
        Status = status;
        Description = description;
    }

    public ListingKey Key { get; }

    public string Id => Key.Id;

    public string Source => Key.Source;

    public string Address { get; }

    public string City { get; }

    public string State { get; }

    public string Zip { get; }

    /// <summary>List price in USD.</summary>
    public decimal Price { get; }

    public int Bedrooms { get; }

    public decimal Bathrooms { get; }

    public int Sqft { get; }

    public double Latitude { get; }

    public double Longitude { get; }

    public DateOnly ListedDate { get; }

    public ListingStatus Status { get; }

    public string Description { get; }

    /// <exception cref="DomainValidationException">Any argument breaks a listing invariant.</exception>
    public static Listing Create(
        string? source,
        string? id,
        string? address,
        string? city,
        string? state,
        string? zip,
        decimal price,
        int bedrooms,
        decimal bathrooms,
        int sqft,
        double latitude,
        double longitude,
        DateOnly listedDate,
        ListingStatus status,
        string? description)
    {
        var key = new ListingKey(Required(source, nameof(source)), Required(id, nameof(id)));

        if (price <= 0)
        {
            throw new DomainValidationException($"Listing {key}: price must be greater than zero.");
        }

        if (bedrooms < 0)
        {
            throw new DomainValidationException($"Listing {key}: bedrooms cannot be negative.");
        }

        if (bathrooms < 0)
        {
            throw new DomainValidationException($"Listing {key}: bathrooms cannot be negative.");
        }

        if (sqft < 0)
        {
            throw new DomainValidationException($"Listing {key}: sqft cannot be negative.");
        }

        if (double.IsNaN(latitude) || latitude is < -90 or > 90)
        {
            throw new DomainValidationException($"Listing {key}: latitude must be between -90 and 90.");
        }

        if (double.IsNaN(longitude) || longitude is < -180 or > 180)
        {
            throw new DomainValidationException($"Listing {key}: longitude must be between -180 and 180.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new DomainValidationException($"Listing {key}: status '{status}' is not recognised.");
        }

        return new Listing(
            key,
            Required(address, nameof(address)),
            Required(city, nameof(city)),
            state?.Trim() ?? string.Empty,
            zip?.Trim() ?? string.Empty,
            price,
            bedrooms,
            bathrooms,
            sqft,
            latitude,
            longitude,
            listedDate,
            status,
            description?.Trim() ?? string.Empty);
    }

    private static string Required(string? value, string field) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new DomainValidationException($"Listing {field} is required.")
            : value.Trim();
}
