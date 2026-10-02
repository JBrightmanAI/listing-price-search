using ListingSearch.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace ListingSearch.IntegrationTests;

/// <summary>
/// Hosts the real application in memory with the real sample data. Only the clock is pinned
/// (so recency scores are stable) and the rate limit raised (so tests do not throttle each other).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public static readonly DateTimeOffset Now = new(2026, 9, 5, 12, 0, 0, TimeSpan.Zero);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TimeProvider>(new FakeTimeProvider(Now));
            services.Configure<RateLimitingOptions>(o => o.PermitLimit = 10_000);
        });
    }
}
