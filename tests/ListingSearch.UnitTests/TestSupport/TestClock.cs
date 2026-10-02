using Microsoft.Extensions.Time.Testing;

namespace ListingSearch.UnitTests.TestSupport;

/// <summary>A fixed "now" so recency-dependent assertions never depend on the day the tests run.</summary>
internal static class TestClock
{
    public static readonly DateTimeOffset Now = new(2026, 9, 5, 12, 0, 0, TimeSpan.Zero);

    public static DateOnly Today => DateOnly.FromDateTime(Now.UtcDateTime);

    public static FakeTimeProvider Create() => new(Now);
}
