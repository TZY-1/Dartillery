using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Observer pattern for throw events.
/// Enables logging, analytics, and bot training data collection.
/// </summary>
public interface IThrowEventListener
{
    /// <summary>
    /// Called when a throw has been completed.
    /// </summary>
    /// <param name="evt">Event data with result, context, and player info.</param>
    void OnThrowCompleted(ThrowEvent evt);
}

/// <summary>
/// Event data for completed throws.
/// </summary>
public sealed record ThrowEvent
{
    /// <summary>
    /// The throw result (required).
    /// </summary>
    public required ThrowResult Result { get; init; }

    /// <summary>
    /// The throw context (required).
    /// </summary>
    public required ThrowContext Context { get; init; }

    /// <summary>
    /// The player profile (required).
    /// </summary>
    public required PlayerProfile Profile { get; init; }

    /// <summary>
    /// Timestamp when throw was executed.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Session identifier.
    /// </summary>
    public long SessionId { get; init; }
}
