namespace Dartillery.Core.Models;

/// <summary>
/// Optional metadata for throw analytics and debugging.
/// Useful for bot training and statistical analysis.
/// </summary>
public sealed record ThrowMetadata
{
    /// <summary>
    /// Systematic bias applied to this throw (X component).
    /// </summary>
    public double SystematicBiasApplied { get; init; }

    /// <summary>
    /// Tremor magnitude at time of throw.
    /// </summary>
    public double TremorMagnitude { get; init; }

    /// <summary>
    /// Pressure modifier applied to this throw.
    /// </summary>
    public double PressureModifier { get; init; }

    /// <summary>
    /// Momentum modifier applied to this throw.
    /// </summary>
    public double MomentumModifier { get; init; }

    /// <summary>
    /// Name of player who made the throw.
    /// </summary>
    public string? PlayerName { get; init; }

    /// <summary>
    /// Session identifier for grouping throws.
    /// </summary>
    public long SessionId { get; init; }

    /// <summary>
    /// Timestamp when throw was executed.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Extensibility dictionary for custom data.
    /// </summary>
    public Dictionary<string, object>? CustomData { get; init; }
}
