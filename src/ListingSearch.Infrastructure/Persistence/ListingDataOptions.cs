namespace ListingSearch.Infrastructure.Persistence;

public sealed class ListingDataOptions
{
    public const string SectionName = "ListingData";

    /// <summary>Path to the listings JSON file. A relative path is resolved against the application directory.</summary>
    public string FilePath { get; set; } = string.Empty;
}
