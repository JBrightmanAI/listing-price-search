namespace ListingSearch.Infrastructure.Persistence;

/// <summary>
/// One listing exactly as it appears in a feed file. Everything is nullable because feed data is
/// untrusted; <see cref="ListingFeedParser"/> decides what is acceptable before a domain object is built.
/// </summary>
internal sealed class ListingRecord
{
    public string? Id { get; set; }

    public string? Source { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public decimal? Price { get; set; }

    public int? Bedrooms { get; set; }

    public decimal? Bathrooms { get; set; }

    public int? Sqft { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? ListedDate { get; set; }

    public string? Status { get; set; }

    public string? Description { get; set; }
}
