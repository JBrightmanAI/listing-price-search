using ListingSearch.Application.Abstractions;
using ListingSearch.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ListingSearch.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the JSON-backed listing store. The host binds <see cref="ListingDataOptions"/> from configuration.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<ListingDataOptions>()
            .Validate(o => !string.IsNullOrWhiteSpace(o.FilePath), "ListingData:FilePath must be configured.")
            .ValidateOnStart();

        services.AddSingleton<JsonListingLoader>();
        services.AddSingleton(sp => new InMemoryListingRepository(sp.GetRequiredService<JsonListingLoader>().Load()));
        services.AddSingleton<IListingRepository>(sp => sp.GetRequiredService<InMemoryListingRepository>());
        services.AddHostedService<ListingDataWarmup>();

        return services;
    }
}
