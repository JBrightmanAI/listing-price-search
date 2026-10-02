namespace ListingSearch.Domain;

/// <summary>Thrown when an attempt is made to create a domain object that would break its invariants.</summary>
public sealed class DomainValidationException : Exception
{
    public DomainValidationException()
    {
    }

    public DomainValidationException(string message)
        : base(message)
    {
    }

    public DomainValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
