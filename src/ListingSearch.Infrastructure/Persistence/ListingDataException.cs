namespace ListingSearch.Infrastructure.Persistence;

/// <summary>The listing data as a whole cannot be used, so the application must not start.</summary>
public sealed class ListingDataException : Exception
{
    public ListingDataException()
    {
    }

    public ListingDataException(string message)
        : base(message)
    {
    }

    public ListingDataException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
