namespace Dartillery.Core.Models;

/// <summary>
/// Context information for a single throw.
/// Contains all dynamic modifiers applied during throw execution.
/// </summary>
public sealed record ThrowContext
{
    /// <summary>
    /// Session-specific tremor magnitude (accumulated fatigue).
    /// Added to base skill to simulate declining accuracy.
    /// </summary>
    public double SessionTremor { get; init; } = 0.0;

    /// <summary>
    /// Pressure-based precision modifier (multiplier).
    /// > 1.0 = under pressure (worse), 1.0 = neutral.
    /// </summary>
    public double PressureModifier { get; init; } = 1.0;

    /// <summary>
    /// Momentum-based precision modifier (multiplier).
    /// < 1.0 = hot hand (better), > 1.0 = cold streak (worse), 1.0 = neutral.
    /// </summary>
    public double MomentumModifier { get; init; } = 1.0;

    /// <summary>
    /// Number of throws executed in current session.
    /// Used to calculate fatigue accumulation.
    /// </summary>
    public int ThrowIndexInSession { get; init; } = 0;

    /// <summary>
    /// Game-specific context (optional, for pressure calculation).
    /// </summary>
    public GameContext? GameContext { get; init; }

    /// <summary>
    /// Previous throws in current visit (for dart grouping effects).
    /// </summary>
    public List<ThrowResult> PreviousThrowsInVisit { get; init; } = new();

    /// <summary>
    /// Creates a neutral context with no modifiers.
    /// </summary>
    public static ThrowContext Neutral => new();
}
