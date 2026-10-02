using ListingSearch.Application.Scoring;
using ListingSearch.Application.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ListingSearch.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the application use cases. The host binds <see cref="ScoringOptions"/> from configuration;
    /// the options are validated when the host starts, so bad weights stop the app before it serves traffic.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<IValidateOptions<ScoringOptions>, ScoringOptionsValidator>();
        services.AddOptions<ScoringOptions>().ValidateOnStart();

        // Everything here is stateless, so singletons are safe and avoid per-request allocation.
        services.AddSingleton<IRelevanceScorer, BudgetRecencyScorer>();
        services.AddSingleton<IListingSearchService, ListingSearchService>();

        return services;
    }
}
