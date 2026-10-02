using Microsoft.Extensions.Hosting;

namespace ListingSearch.Infrastructure.Persistence;

/// <summary>
/// Forces the listing data to load while the host starts, before the server accepts requests.
/// Taking the repository as a dependency is what triggers the load, so a bad data file stops startup
/// instead of failing the first user's search.
/// </summary>
internal sealed class ListingDataWarmup(InMemoryListingRepository repository) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = repository.Count;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
