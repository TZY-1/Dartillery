namespace Dartillery.Web.Models;

/// <summary>
/// Static lookup for dart checkout feasibility.
/// Determines whether a remaining score can be finished with a given number of darts.
/// </summary>
public static class CheckoutTable
{
    /// <summary>
    /// Maximum checkout with 1 dart: D20 (40) or D-Bull (50).
    /// </summary>
    private const int _maxOneDartCheckout = 50;

    /// <summary>
    /// Maximum checkout with 2 darts: T20 (60) + D-Bull (50) = 110.
    /// </summary>
    private const int _maxTwoDartCheckout = 110;

    /// <summary>
    /// Maximum checkout with 3 darts: T20 + T20 + D-Bull = 170.
    /// </summary>
    private const int _maxThreeDartCheckout = 170;

    /// <summary>
    /// Scores that cannot be checked out in any number of darts (bogey numbers).
    /// These have no valid path ending on a double.
    /// </summary>
    private static readonly HashSet<int> _bogeyNumbers = [169, 168, 166, 165, 163, 162, 159];

    /// <summary>
    /// All valid double-out values (D1–D20 + D-Bull).
    /// </summary>
    private static readonly HashSet<int> _doubleOuts =
    [
        2, 4, 6, 8, 10, 12, 14, 16, 18, 20,
        22, 24, 26, 28, 30, 32, 34, 36, 38, 40,
        50 // Bullseye (double bull)
    ];

    /// <summary>
    /// Returns whether a score can be checked out with the given number of remaining darts.
    /// </summary>
    public static bool IsCheckable(int remainingScore, int dartsRemaining)
    {
        if (remainingScore <= 0 || remainingScore == 1)
            return false;

        if (_bogeyNumbers.Contains(remainingScore))
            return false;

        return dartsRemaining switch
        {
            >= 3 => remainingScore <= _maxThreeDartCheckout,
            2 => remainingScore <= _maxTwoDartCheckout,
            1 => remainingScore <= _maxOneDartCheckout && _doubleOuts.Contains(remainingScore),
            _ => false
        };
    }

    /// <summary>
    /// Returns whether a score is a bogey number (no clean finish possible).
    /// </summary>
    public static bool IsBogey(int remainingScore) => _bogeyNumbers.Contains(remainingScore);

    /// <summary>
    /// Returns whether a score requires a double to finish (standard darts rules).
    /// Scores of 1 are unfinishable (no half-value double exists).
    /// </summary>
    public static bool IsFinishable(int remainingScore) =>
        remainingScore >= 2 && remainingScore <= _maxThreeDartCheckout && !_bogeyNumbers.Contains(remainingScore);
}
