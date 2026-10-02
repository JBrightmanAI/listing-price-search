using ListingSearch.Application.Scoring;
using ListingSearch.UnitTests.TestSupport;
using Microsoft.Extensions.Options;

namespace ListingSearch.UnitTests.Scoring;

public sealed class BudgetRecencyScorerTests
{
    private static readonly BudgetRecencyScorer DefaultScorer = new(Options.Create(new ScoringOptions()));

    private static ScoringContext Context(decimal? targetBudget) => new(targetBudget, TestClock.Today);

    [Fact]
    public void BudgetFit_is_one_at_the_target()
    {
        Assert.Equal(1.0, BudgetRecencyScorer.BudgetFit(500_000m, 500_000m));
    }

    [Fact]
    public void BudgetFit_treats_over_and_under_budget_the_same()
    {
        var under = BudgetRecencyScorer.BudgetFit(450_000m, 500_000m);
        var over = BudgetRecencyScorer.BudgetFit(550_000m, 500_000m);

        Assert.Equal(0.9, under, precision: 10);
        Assert.Equal(under, over);
    }

    [Theory]
    [InlineData(1_000_000)]
    [InlineData(5_000_000)]
    public void BudgetFit_is_zero_when_price_is_100_percent_or_more_away(decimal price)
    {
        Assert.Equal(0.0, BudgetRecencyScorer.BudgetFit(price, 500_000m));
    }

    [Fact]
    public void BudgetFit_does_not_overflow_for_a_tiny_target()
    {
        Assert.Equal(0.0, BudgetRecencyScorer.BudgetFit(79_000_000_000m, 0.0000001m));
    }

    [Theory]
    [InlineData(0, 1.0)]
    [InlineData(30, 0.5)]
    [InlineData(60, 0.25)]
    public void RecencyScore_halves_every_half_life(int daysAgo, double expected)
    {
        var score = BudgetRecencyScorer.RecencyScore(TestClock.Today.AddDays(-daysAgo), TestClock.Today, 30);

        Assert.Equal(expected, score, precision: 10);
    }

    [Fact]
    public void RecencyScore_treats_a_future_listing_as_listed_today()
    {
        Assert.Equal(1.0, BudgetRecencyScorer.RecencyScore(TestClock.Today.AddDays(5), TestClock.Today, 30));
    }

    [Fact]
    public void Score_without_target_budget_is_recency_only()
    {
        var listing = new ListingBuilder().ListedDaysAgo(30).Build();

        Assert.Equal(50.0, DefaultScorer.Score(listing, Context(null)));
    }

    [Fact]
    public void Score_combines_budget_and_recency_by_weight()
    {
        // budget fit 1.0 * 0.7 + recency 0.5 * 0.3 = 0.85
        var listing = new ListingBuilder().WithPrice(500_000m).ListedDaysAgo(30).Build();

        Assert.Equal(85.0, DefaultScorer.Score(listing, Context(500_000m)));
    }

    [Fact]
    public void Score_uses_configured_weights()
    {
        var budgetOnly = new BudgetRecencyScorer(Options.Create(new ScoringOptions { BudgetWeight = 1, RecencyWeight = 0 }));
        var listing = new ListingBuilder().WithPrice(450_000m).ListedDaysAgo(90).Build();

        Assert.Equal(90.0, budgetOnly.Score(listing, Context(500_000m)));
    }

    [Fact]
    public void Score_is_rounded_to_two_decimals()
    {
        var listing = new ListingBuilder().ListedDaysAgo(7).Build();

        var score = DefaultScorer.Score(listing, Context(null));

        Assert.Equal(Math.Round(score, 2), score);
    }

    [Fact]
    public void Score_stays_within_zero_and_hundred()
    {
        var best = new ListingBuilder().WithPrice(500_000m).ListedDaysAgo(0).Build();
        var worst = new ListingBuilder().WithPrice(5_000_000m).ListedDaysAgo(10_000).Build();

        Assert.Equal(100.0, DefaultScorer.Score(best, Context(500_000m)));
        Assert.Equal(0.0, DefaultScorer.Score(worst, Context(500_000m)));
    }
}
