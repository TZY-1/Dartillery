namespace Dartillery.Core.Models;

/// <summary>
/// Session state for tremor calculation.
/// Captures temporal aspects of a throwing session.
/// </summary>
public sealed record SessionState
{
    /// <summary>
    /// Total number of throws executed in current session.
    /// </summary>
    public int ThrowCount { get; init; }

    /// <summary>
    /// Total duration of current session.
    /// </summary>
    public TimeSpan SessionDuration { get; init; }

    /// <summary>
    /// Time elapsed since last throw (for recovery calculations).
    /// </summary>
    public TimeSpan TimeSinceLastThrow { get; init; }

    /// <summary>
    /// Current tremor magnitude from previous calculation.
    /// Used by recovery models to track tremor decay.
    /// </summary>
    public double CurrentTremor { get; init; }
}
