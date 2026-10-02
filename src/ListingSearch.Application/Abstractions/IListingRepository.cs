using ListingSearch.Domain;

namespace ListingSearch.Application.Abstractions;

/// <summary>
/// Read access to listings. The dataset here is small and held in memory, so the service filters in process.
/// A database-backed implementation would push filtering and paging down into the query instead.
/// </summary>
public interface IListingRepository
{
    Task<IReadOnlyCollection<Listing>> GetAllAsync(CancellationToken cancellationToken);
}
