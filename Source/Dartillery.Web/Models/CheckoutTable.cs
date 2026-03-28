namespace Dartillery.Web.Models;

/// <summary>
/// Static lookup for dart checkout feasibility.
/// Determines whether a remaining score can be finished with a given number of darts.
/// </summary>
public static class CheckoutTable
{
    /// <summary>
    /// Scores that cannot be checked out in any number of darts (bogey numbers).
    /// These have no valid path ending on a double.
    /// </summary>
    private static readonly HashSet<int> BogeyNumbers = [169, 168, 166, 165, 163, 162, 159];

    /// <summary>
    /// All valid double-out values (D1–D20 + D-Bull).
    /// </summary>
    private static readonly HashSet<int> DoubleOuts =
    [
        2, 4, 6, 8, 10, 12, 14, 16, 18, 20,
        22, 24, 26, 28, 30, 32, 34, 36, 38, 40,
        50 // Bullseye (double bull)
    ];

    /// <summary>
    /// Maximum checkout with 1 dart: D20 (40) or D-Bull (50).
    /// </summary>
    private const int MaxOneDartCheckout = 50;

    /// <summary>
    /// Maximum checkout with 2 darts: T20 (60) + D-Bull (50) = 110.
    /// </summary>
    private const int MaxTwoDartCheckout = 110;

    /// <summary>
    /// Maximum checkout with 3 darts: T20 + T20 + D-Bull = 170.
    /// </summary>
    private const int MaxThreeDartCheckout = 170;

    /// <summary>
    /// Returns whether a score can be checked out with the given number of remaining darts.
    /// </summary>
    public static bool IsCheckable(int remainingScore, int dartsRemaining)
    {
        if (remainingScore <= 0 || remainingScore == 1)
            return false;

        if (BogeyNumbers.Contains(remainingScore))
            return false;

        return dartsRemaining switch
        {
            >= 3 => remainingScore <= MaxThreeDartCheckout,
            2 => remainingScore <= MaxTwoDartCheckout,
            1 => remainingScore <= MaxOneDartCheckout && DoubleOuts.Contains(remainingScore),
            _ => false
        };
    }

    /// <summary>
    /// Returns whether a score is a bogey number (no clean finish possible).
    /// </summary>
    public static bool IsBogey(int remainingScore) => BogeyNumbers.Contains(remainingScore);

    /// <summary>
    /// Returns whether a score requires a double to finish (standard darts rules).
    /// Scores of 1 are unfinishable (no half-value double exists).
    /// </summary>
    public static bool IsFinishable(int remainingScore) =>
        remainingScore >= 2 && remainingScore <= MaxThreeDartCheckout && !BogeyNumbers.Contains(remainingScore);
}
