using System.Net;
using ListingSearch.Api.Infrastructure;
using ListingSearch.Application.Common;
using ListingSearch.Application.Search;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ListingSearch.IntegrationTests;

/// <summary>Production concerns: health, security headers, error handling, rate limiting and startup checks.</summary>
public sealed class OperationalTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly Uri SearchUri = new("/api/listings/search", UriKind.Relative);

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_endpoints_report_healthy(string path)
    {
        var response = await factory.CreateClient().GetAsync(new Uri(path, UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Responses_carry_security_headers_and_hide_the_server()
    {
        var response = await factory.CreateClient().GetAsync(SearchUri);

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        Assert.False(response.Headers.Contains("Server"));
    }

    [Fact]
    public async Task Unknown_api_route_is_a_404_problem()
    {
        var response = await factory.CreateClient().GetAsync(new Uri("/api/does-not-exist", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Unhandled_exception_is_a_generic_500_without_internal_details()
    {
        using var failingFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddSingleton<IListingSearchService, ThrowingSearchService>()));

        var response = await failingFactory.CreateClient().GetAsync(SearchUri);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("traceId", body, StringComparison.Ordinal);
        Assert.DoesNotContain(ThrowingSearchService.SecretMessage, body, StringComparison.Ordinal);
        Assert.DoesNotContain("   at ", body, StringComparison.Ordinal);

        // Security headers must survive the exception handler clearing the response.
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
    }

    [Fact]
    public async Task Exceeding_the_rate_limit_is_a_429_with_retry_after()
    {
        using var limitedFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.Configure<RateLimitingOptions>(o =>
            {
                o.PermitLimit = 2;
                o.WindowSeconds = 60;
            })));
        var client = limitedFactory.CreateClient();

        await client.GetAsync(SearchUri);
        await client.GetAsync(SearchUri);
        var throttled = await client.GetAsync(SearchUri);

        Assert.Equal(HttpStatusCode.TooManyRequests, throttled.StatusCode);
        Assert.True(throttled.Headers.Contains("Retry-After"));
        Assert.Equal("application/problem+json", throttled.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Health_checks_are_not_rate_limited()
    {
        using var limitedFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.Configure<RateLimitingOptions>(o => o.PermitLimit = 1)));
        var client = limitedFactory.CreateClient();

        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(new Uri("/health/live", UriKind.Relative))).StatusCode);
        }
    }

    [Fact]
    public void Startup_fails_when_the_data_file_is_missing()
    {
        using var brokenFactory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("ListingData:FilePath", "Data/does-not-exist.json"));

        var exception = Assert.ThrowsAny<Exception>(() => brokenFactory.CreateClient());
        Assert.Contains("could not be read", exception.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Startup_fails_when_scoring_weights_are_invalid()
    {
        using var brokenFactory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Scoring:BudgetWeight", "0.9"));

        var exception = Assert.ThrowsAny<Exception>(() => brokenFactory.CreateClient());
        Assert.Contains("must add up to 1", exception.ToString(), StringComparison.Ordinal);
    }

    private sealed class ThrowingSearchService : IListingSearchService
    {
        public const string SecretMessage = "connection string=secret";

        public Task<Result<PagedResult<ScoredListing>>> SearchAsync(SearchListingsQuery query, CancellationToken cancellationToken) =>
            throw new InvalidOperationException(SecretMessage);
    }
}
