namespace Dartillery.Core.Models;

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
