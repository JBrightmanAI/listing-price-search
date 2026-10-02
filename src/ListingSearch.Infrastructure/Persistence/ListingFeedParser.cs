using System.Globalization;
using System.Text.Json;
using ListingSearch.Domain;

namespace ListingSearch.Infrastructure.Persistence;

/// <param name="Index">Zero-based position of the record in the feed.</param>
/// <param name="Identifier">The record's source and id when they could be read, otherwise its position.</param>
public sealed record RejectedRecord(int Index, string Identifier, string Reason);

public sealed record FeedParseResult(IReadOnlyList<Listing> Listings, IReadOnlyList<RejectedRecord> Rejected);

/// <summary>
/// Turns a feed document into domain listings. One bad record is rejected with a reason rather than
/// failing the whole feed; only a document that is not a JSON array at all is fatal.
/// </summary>
public static class ListingFeedParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <exception cref="ListingDataException">The document is not valid JSON or its root is not an array.</exception>
    public static FeedParseResult Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new ListingDataException("Listing data is not valid JSON.", ex);
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new ListingDataException("Listing data must be a JSON array of listings.");
            }

            var listings = new List<Listing>();
            var rejected = new List<RejectedRecord>();
            var seenKeys = new HashSet<ListingKey>();
            var index = 0;

            foreach (var element in document.RootElement.EnumerateArray())
            {
                var identifier = $"#{index}";
                try
                {
                    var record = element.Deserialize<ListingRecord>(SerializerOptions)
                        ?? throw new DomainValidationException("Record is null.");

                    if (!string.IsNullOrWhiteSpace(record.Source) && !string.IsNullOrWhiteSpace(record.Id))
                    {
                        identifier = $"{record.Source.Trim()}:{record.Id.Trim()}";
                    }

                    var listing = ToListing(record);

                    if (seenKeys.Add(listing.Key))
                    {
                        listings.Add(listing);
                    }
                    else
                    {
                        rejected.Add(new RejectedRecord(index, identifier, "Duplicate source and id; the first occurrence was kept."));
                    }
                }
                catch (JsonException ex)
                {
                    rejected.Add(new RejectedRecord(index, identifier, $"Field has the wrong type: {ex.Path ?? "unknown"}."));
                }
                catch (DomainValidationException ex)
                {
                    rejected.Add(new RejectedRecord(index, identifier, ex.Message));
                }

                index++;
            }

            return new FeedParseResult(listings, rejected);
        }
    }

    private static Listing ToListing(ListingRecord record) => Listing.Create(
        record.Source,
        record.Id,
        record.Address,
        record.City,
        record.State,
        record.Zip,
        Required(record.Price, "price"),
        Required(record.Bedrooms, "bedrooms"),
        Required(record.Bathrooms, "bathrooms"),
        Required(record.Sqft, "sqft"),
        Required(record.Latitude, "latitude"),
        Required(record.Longitude, "longitude"),
        ParseDate(record.ListedDate),
        ParseStatus(record.Status),
        record.Description);

    private static T Required<T>(T? value, string field)
        where T : struct =>
        value ?? throw new DomainValidationException($"Listing {field} is required.");

    /// <summary>Accepts an ISO date ("2026-08-29") or an ISO date-time, which is reduced to its UTC date.</summary>
    private static DateOnly ParseDate(string? value)
    {
        var text = value?.Trim();

        if (DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTime))
        {
            return DateOnly.FromDateTime(dateTime.UtcDateTime);
        }

        throw new DomainValidationException($"Listing listedDate '{value}' is not an ISO date.");
    }

    private static ListingStatus ParseStatus(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "ACTIVE" => ListingStatus.Active,
        "PENDING" => ListingStatus.Pending,
        "SOLD" => ListingStatus.Sold,
        _ => throw new DomainValidationException($"Listing status '{value}' is not one of active, pending, sold."),
    };
}
