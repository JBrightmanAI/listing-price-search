using ListingSearch.Domain;
using Microsoft.Extensions.Options;

namespace ListingSearch.Application.Scoring;

/// <summary>
/// Scores a listing by how close its price is to the target budget and how recently it was listed.
/// <code>
/// budgetFit = max(0, 1 - |price - target| / target)        1.0 at the target, 0 at 100% away or more
/// recency   = 0.5 ^ (daysOnMarket / halfLifeDays)           1.0 today, 0.5 after one half-life
/// score     = 100 * (budgetWeight * budgetFit + recencyWeight * recency)
/// </code>
/// Without a target budget the score is recency alone, scaled to 100.
/// The score is rounded to two decimals so that listings which look tied are ordered by the tie-break rules.
/// </summary>
internal sealed class BudgetRecencyScorer(IOptions<ScoringOptions> options) : IRelevanceScorer
{
    private const int ScoreDecimals = 2;

    private readonly ScoringOptions _options = options.Value;

    public double Score(Listing listing, ScoringContext context)
    {
        ArgumentNullException.ThrowIfNull(listing);
        ArgumentNullException.ThrowIfNull(context);

        var recency = RecencyScore(listing.ListedDate, context.Today, _options.RecencyHalfLifeDays);

        var raw = context.TargetBudget is { } target
            ? (_options.BudgetWeight * BudgetFit(listing.Price, target)) + (_options.RecencyWeight * recency)
            : recency;

        return Math.Round(raw * 100, ScoreDecimals, MidpointRounding.AwayFromZero);
    }

    internal static double BudgetFit(decimal price, decimal targetBudget)
    {
        if (targetBudget <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetBudget), "Target budget must be positive.");
        }

        var deviation = Math.Abs(price - targetBudget);

        // Compare before dividing: a tiny target with a large deviation would otherwise overflow decimal.
        if (deviation >= targetBudget)
        {
            return 0;
        }

        return (double)(1 - (deviation / targetBudget));
    }

    internal static double RecencyScore(DateOnly listedDate, DateOnly today, double halfLifeDays)
    {
        // A listing dated in the future (clock skew, bad feed data) is treated as listed today.
        var daysOnMarket = Math.Max(0, today.DayNumber - listedDate.DayNumber);
        return Math.Pow(0.5, daysOnMarket / halfLifeDays);
    }
}
