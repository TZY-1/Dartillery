namespace Dartillery.Web.Models;

/// <summary>
/// Predefined pressure scenarios for testing different game situations.
/// </summary>
public static class PressureScenarios
{
    public static PressureScenario Relaxed { get; } =
        new("Relaxed", "No pressure — mid-game throw", 501, false, false, 0);

    public static PressureScenario CheckoutD16 { get; } =
        new("Checkout D16", "32 remaining, first attempt", 32, true, false, 0);

    public static PressureScenario CheckoutD16Retry { get; } =
        new("Checkout D16 (retry)", "32 remaining, 2nd attempt", 32, true, false, 2);

    public static PressureScenario MatchPoint170 { get; } =
        new("Match Point 170", "High checkout under match pressure", 170, true, true, 0);

    public static PressureScenario Bogey169 { get; } =
        new("Bogey 169", "Awkward finish — no clean double-out", 169, true, false, 0);

    public static PressureScenario ScoreAnxiety { get; } =
        new("Score Anxiety", "38 remaining, fear of busting", 38, true, false, 1);

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
