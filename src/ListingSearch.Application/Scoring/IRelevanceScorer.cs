using ListingSearch.Domain;

namespace ListingSearch.Application.Scoring;

/// <param name="TargetBudget">The buyer's budget, or null when none was given.</param>
/// <param name="Today">The reference date for recency, taken from the injected clock.</param>
public sealed record ScoringContext(decimal? TargetBudget, DateOnly Today);

public interface IRelevanceScorer
{
    /// <returns>A relevance score from 0 to 100, rounded to two decimal places. Higher is more relevant.</returns>
    double Score(Listing listing, ScoringContext context);
}
