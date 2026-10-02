using ListingSearch.Application.Scoring;

namespace ListingSearch.UnitTests.Scoring;

public sealed class ScoringOptionsValidatorTests
{
    private readonly ScoringOptionsValidator _validator = new();

    [Fact]
    public void Defaults_are_valid()
    {
        Assert.True(_validator.Validate(null, new ScoringOptions()).Succeeded);
    }

    [Theory]
    [InlineData(0.5, 0.4, 30)]   // weights do not add up to 1
    [InlineData(1.2, -0.2, 30)]  // weight out of range
    [InlineData(double.NaN, 0.3, 30)]
    [InlineData(0.7, 0.3, 0)]    // half-life must be positive
    [InlineData(0.7, 0.3, double.PositiveInfinity)]
    public void Invalid_settings_fail(double budgetWeight, double recencyWeight, double halfLife)
    {
        var options = new ScoringOptions
        {
            BudgetWeight = budgetWeight,
            RecencyWeight = recencyWeight,
            RecencyHalfLifeDays = halfLife,
        };

        Assert.True(_validator.Validate(null, options).Failed);
    }
}
