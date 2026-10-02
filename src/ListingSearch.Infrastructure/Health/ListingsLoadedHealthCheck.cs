using ListingSearch.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ListingSearch.Infrastructure.Health;

/// <summary>Readiness: the service can only answer searches once listings are loaded.</summary>
public sealed class ListingsLoadedHealthCheck(InMemoryListingRepository repository) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object> { ["listingCount"] = repository.Count };

        return Task.FromResult(repository.Count > 0
            ? HealthCheckResult.Healthy("Listings are loaded.", data)
            : HealthCheckResult.Unhealthy("No listings are loaded.", data: data));
    }
}
