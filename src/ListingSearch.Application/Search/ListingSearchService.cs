using System.Diagnostics;
using ListingSearch.Application.Abstractions;
using ListingSearch.Application.Common;
using ListingSearch.Application.Scoring;
using Microsoft.Extensions.Logging;

namespace ListingSearch.Application.Search;

public interface IListingSearchService
{
    Task<Result<PagedResult<ScoredListing>>> SearchAsync(SearchListingsQuery query, CancellationToken cancellationToken);
}

/// <summary>
/// The search use case as a pipeline: validate, filter, score, rank, paginate.
/// Each step is separate so a new step (for example cross-feed de-duplication between load and filter)
/// can be added without changing the others.
/// </summary>
internal sealed partial class ListingSearchService(
    IListingRepository repository,
    IRelevanceScorer scorer,
    TimeProvider timeProvider,
    ILogger<ListingSearchService> logger) : IListingSearchService
{
    public async Task<Result<PagedResult<ScoredListing>>> SearchAsync(
        SearchListingsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var errors = SearchListingsQueryValidator.Validate(query);
        if (errors.HasErrors)
        {
            return Result.Invalid<PagedResult<ScoredListing>>(errors);
        }

        var started = Stopwatch.GetTimestamp();

        var filter = ListingFilter.From(query);
        var context = new ScoringContext(query.TargetBudget, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));

        var listings = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var ranked = listings
            .Where(filter.Matches)
            .Select(listing => new ScoredListing(listing, scorer.Score(listing, context)))
            .Order(ListingRankComparer.Instance)
            .ToList();

        var page = Paginate(ranked, query.Page, query.PageSize);

        if (logger.IsEnabled(LogLevel.Information))
        {
            var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            LogSearchCompleted(logger, page.TotalCount, page.Page, page.Items.Count, elapsedMs);
        }

        return Result.Success(page);
    }

    private static PagedResult<ScoredListing> Paginate(List<ScoredListing> ranked, int page, int pageSize)
    {
        // 64-bit offset: page * pageSize can exceed int.MaxValue for a hostile or mistaken page number.
        var offset = (long)(page - 1) * pageSize;

        IReadOnlyList<ScoredListing> items = offset >= ranked.Count
            ? []
            : ranked.GetRange((int)offset, (int)Math.Min(pageSize, ranked.Count - offset));

        return new PagedResult<ScoredListing>(items, page, pageSize, ranked.Count);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Search matched {TotalCount} listings, returned page {Page} with {ItemCount} items in {ElapsedMs:F1} ms")]
    private static partial void LogSearchCompleted(ILogger logger, int totalCount, int page, int itemCount, double elapsedMs);
}
