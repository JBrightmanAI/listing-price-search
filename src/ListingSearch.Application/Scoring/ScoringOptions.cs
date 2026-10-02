using Microsoft.Extensions.Options;

namespace ListingSearch.Application.Scoring;

/// <summary>Tunable weights for <see cref="BudgetRecencyScorer"/>. Bound from the "Scoring" configuration section.</summary>
public sealed class ScoringOptions
{
    public const string SectionName = "Scoring";

    /// <summary>Share of the score that comes from how close the price is to the target budget.</summary>
    public double BudgetWeight { get; set; } = 0.7;

    /// <summary>Share of the score that comes from how recently the listing went live.</summary>
    public double RecencyWeight { get; set; } = 0.3;

    /// <summary>Days after which the recency component halves.</summary>
    public double RecencyHalfLifeDays { get; set; } = 30;
}

/// <summary>Rejects a bad scoring configuration at startup rather than producing nonsense scores at runtime.</summary>
internal sealed class ScoringOptionsValidator : IValidateOptions<ScoringOptions>
{
    private const double WeightTolerance = 1e-9;

    public ValidateOptionsResult Validate(string? name, ScoringOptions options)
    {
        var failures = new List<string>();

        if (!IsWeight(options.BudgetWeight))
        {
            failures.Add($"{nameof(ScoringOptions.BudgetWeight)} must be between 0 and 1.");
        }

        if (!IsWeight(options.RecencyWeight))
        {
            failures.Add($"{nameof(ScoringOptions.RecencyWeight)} must be between 0 and 1.");
        }

        if (Math.Abs(options.BudgetWeight + options.RecencyWeight - 1) > WeightTolerance)
        {
            failures.Add("BudgetWeight and RecencyWeight must add up to 1.");
        }

        if (!double.IsFinite(options.RecencyHalfLifeDays) || options.RecencyHalfLifeDays <= 0)
        {
            failures.Add($"{nameof(ScoringOptions.RecencyHalfLifeDays)} must be greater than 0.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsWeight(double value) => double.IsFinite(value) && value is >= 0 and <= 1;
}
