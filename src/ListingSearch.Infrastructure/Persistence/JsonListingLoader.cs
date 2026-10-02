using ListingSearch.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ListingSearch.Infrastructure.Persistence;

/// <summary>Reads the listing file once at startup and logs every record it had to reject.</summary>
internal sealed partial class JsonListingLoader(IOptions<ListingDataOptions> options, ILogger<JsonListingLoader> logger)
{
    /// <exception cref="ListingDataException">The file is missing, unreadable, malformed, or has no valid listings.</exception>
    public IReadOnlyList<Listing> Load()
    {
        var path = ResolvePath(options.Value.FilePath);

        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new ListingDataException($"Listing data file '{path}' could not be read.", ex);
        }

        var result = ListingFeedParser.Parse(json);

        foreach (var rejected in result.Rejected)
        {
            LogRecordRejected(logger, rejected.Index, rejected.Identifier, rejected.Reason);
        }

        if (result.Listings.Count == 0)
        {
            throw new ListingDataException($"Listing data file '{path}' contains no valid listings.");
        }

        LogLoaded(logger, result.Listings.Count, result.Rejected.Count, path);
        return result.Listings;
    }

    private static string ResolvePath(string configuredPath) =>
        Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath));

    [LoggerMessage(Level = LogLevel.Warning, Message = "Rejected listing record {Index} ({Identifier}): {Reason}")]
    private static partial void LogRecordRejected(ILogger logger, int index, string identifier, string reason);

    [LoggerMessage(Level = LogLevel.Information, Message = "Loaded {Count} listings ({RejectedCount} rejected) from {Path}")]
    private static partial void LogLoaded(ILogger logger, int count, int rejectedCount, string path);
}
