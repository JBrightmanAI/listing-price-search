using ListingSearch.Application.Abstractions;
using ListingSearch.Domain;

namespace ListingSearch.Infrastructure.Persistence;

/// <summary>
/// Holds listings in an immutable array loaded once at startup. Reads need no locking,
/// so the repository is safe to share across concurrent requests.
/// </summary>
public sealed class InMemoryListingRepository : IListingRepository
{
    private readonly Listing[] _listings;

    public InMemoryListingRepository(IEnumerable<Listing> listings)
    {
        ArgumentNullException.ThrowIfNull(listings);
        _listings = [.. listings];
    }

    public int Count => _listings.Length;

    public Task<IReadOnlyCollection<Listing>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyCollection<Listing>>(Array.AsReadOnly(_listings));
    }
}
