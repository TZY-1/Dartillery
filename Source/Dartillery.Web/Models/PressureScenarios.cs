namespace Dartillery.Web.Models;

/// <summary>
/// Predefined pressure scenarios for testing different game situations.
/// </summary>
public static class PressureScenarios
{
    /// <summary>No pressure — mid-game free practice.</summary>
    public static PressureScenario Relaxed { get; } =
        new("Relaxed", "No pressure — mid-game throw", 501, false, false, 0);

    /// <summary>Checkout attempt on D16, first try.</summary>
    public static PressureScenario CheckoutD16 { get; } =
        new("Checkout D16", "32 remaining, first attempt", 32, true, false, 0);

    /// <summary>Retry checkout on D16 after missed attempts.</summary>
    public static PressureScenario CheckoutD16Retry { get; } =
        new("Checkout D16 (retry)", "32 remaining, 2nd attempt", 32, true, false, 2);

    /// <summary>Match point at 170 — maximum pressure.</summary>
    public static PressureScenario MatchPoint170 { get; } =
        new("Match Point 170", "High checkout under match pressure", 170, true, true, 0);

    /// <summary>Bogey number 169 — no clean double-out path.</summary>
    public static PressureScenario Bogey169 { get; } =
        new("Bogey 169", "Awkward finish — no clean double-out", 169, true, false, 0);

    /// <summary>Low remaining score with bust risk.</summary>
    public static PressureScenario ScoreAnxiety { get; } =
        new("Score Anxiety", "38 remaining, fear of busting", 38, true, false, 1);

    /// <summary>All predefined pressure scenarios.</summary>
    public static IReadOnlyList<PressureScenario> All { get; } =
    [
        Relaxed,
        CheckoutD16,
        CheckoutD16Retry,
        MatchPoint170,
        Bogey169,
        ScoreAnxiety
    ];
}
