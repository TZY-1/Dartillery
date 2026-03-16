namespace Dartillery.Core.Models;

/// <summary>
/// Immutable event payload dispatched to <see cref="Dartillery.Core.Abstractions.IThrowEventListener"/> instances after each simulated throw.
/// </summary>
public sealed record ThrowEvent
{
    /// <summary>The outcome of the throw: segment hit, score, and coordinates.</summary>
    public required ThrowResult Result { get; init; }

    /// <summary>The behavioral modifiers (tremor, pressure, momentum) active when the throw was made.</summary>
    public required ThrowContext Context { get; init; }

    /// <summary>The player profile used for the throw.</summary>
    public required PlayerProfile Profile { get; init; }

    /// <summary>UTC timestamp recorded when the throw was simulated.</summary>
    public DateTime Timestamp { get; init; }

    /// <summary>Session identifier matching the owning session's ID for cross-throw correlation.</summary>
    public Guid SessionId { get; init; }
}
