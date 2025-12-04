namespace Dartillery.Core.Models;

/// <summary>
/// Game state information for pressure calculation and strategy decisions.
/// </summary>
public sealed record GameContext
{
    /// <summary>
    /// Remaining score in current game (e.g., 501 countdown).
    /// </summary>
    public int RemainingScore { get; init; }

    /// <summary>
    /// Number of checkout attempts for current score.
    /// Multiple failures increase pressure.
    /// </summary>
    public int CheckoutAttempts { get; init; } = 0;

    /// <summary>
    /// Is this throw attempting to finish the game?
    /// </summary>
    public bool IsCheckoutAttempt { get; init; } = false;

    /// <summary>
    /// Is this a match-point situation (high pressure)?
    /// </summary>
    public bool IsMatchPoint { get; init; } = false;

    /// <summary>
    /// Number of throws remaining in current visit (turn).
    /// Typically 0-2 for standard darts.
    /// </summary>
    public int ThrowsRemainingInVisit { get; init; } = 3;

    /// <summary>
    /// Results from previous throws in current visit.
    /// </summary>
    public List<ThrowResult> CurrentVisitResults { get; init; } = new();

    /// <summary>
    /// Recent accuracy metric (percentage of intended targets hit).
    /// Range: 0.0 to 1.0
    /// </summary>
    public double RecentAccuracy { get; init; } = 0.5;

    /// <summary>
    /// Opponent's current score (for match context).
    /// </summary>
    public int OpponentScore { get; init; } = 501;
}
