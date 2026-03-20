namespace Dartillery.Core.Models;

/// <summary>
/// Optional per-throw snapshot of all modifier values applied during simulation, intended for analytics and bot training.
/// </summary>
public sealed record ThrowMetadata
{
    /// <summary>Net horizontal systematic bias (meters) applied by <c>SystematicBiasDeviationCalculator</c>.</summary>
    public double SystematicBiasApplied { get; init; }

    /// <summary>Tremor magnitude (sigma increase) active at the moment of the throw.</summary>
    public double TremorMagnitude { get; init; }

    /// <summary>Pressure multiplier applied to deviation (&gt;1.0 = worse accuracy under pressure).</summary>
    public double PressureModifier { get; init; }

    /// <summary>Momentum multiplier applied to deviation (&lt;1.0 = hot streak, &gt;1.0 = cold streak).</summary>
    public double MomentumModifier { get; init; }

    /// <summary>Grouping multiplier applied when nearby darts cause blocking (&gt;1.0 = blocked).</summary>
    public double GroupingMultiplier { get; init; } = 1.0;

    /// <summary>Target difficulty multiplier based on segment size (&gt;1.0 = harder target).</summary>
    public double DifficultyMultiplier { get; init; } = 1.0;

    /// <summary>Name of the player who made the throw, sourced from <see cref="PlayerProfile.Name"/>.</summary>
    public string? PlayerName { get; init; }

    /// <summary>Session identifier matching the owning session's ID for cross-throw correlation.</summary>
    public Guid SessionId { get; init; }

    /// <summary>UTC timestamp recorded when the throw was simulated.</summary>
    public DateTime Timestamp { get; init; }

    /// <summary>Optional extensibility bag for consumer-defined throw data. Keys are strings; values are arbitrary objects.</summary>
    public Dictionary<string, object>? CustomData { get; init; }
}
