using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;
using Dartillery.Simulation.Models.FatigueModels;
using Dartillery.Simulation.Models.GroupingModels;
using Dartillery.Simulation.Models.MomentumModels;
using Dartillery.Simulation.Models.PressureModels;
using Dartillery.Simulation.Models.TargetDifficultyModels;

namespace Dartillery.Session;

/// <summary>
/// Immutable snapshot of all builder state captured at the moment <c>BuildSession()</c> is called.
/// Acts as the single source of truth for default values, preventing builder-field mutation.
/// </summary>
internal sealed record SessionConfiguration
{
    /// <summary>The player profile defining skill characteristics and biases.</summary>
    public PlayerProfile Profile { get; init; } = PlayerProfile.Amateur();

    /// <summary>The fatigue model used for fatigue simulation.</summary>
    public IFatigueModel FatigueModel { get; init; } = new LinearFatigueModel();

    /// <summary>The pressure model used for psychological effects.</summary>
    public IPressureModel PressureModel { get; init; } = new NoPressureModel();

    /// <summary>The momentum model used for hot/cold streak effects.</summary>
    public IMomentumModel MomentumModel { get; init; } = new NoMomentumModel();

    /// <summary>The grouping model used for dart clustering effects.</summary>
    public IGroupingModel GroupingModel { get; init; } = new NoGroupingModel();

    /// <summary>The target difficulty model used for target-specific difficulty adjustments.</summary>
    public ITargetDifficultyModel TargetDifficultyModel { get; init; } = new NoTargetDifficultyModel();

    /// <summary>The spread calculation mode (Gaussian or Uniform).</summary>
    public SpreadMode SpreadMode { get; init; } = SpreadMode.Gaussian;

    /// <summary>Whether deviation truncation is enabled.</summary>
    public bool UseTruncation { get; init; }

    /// <summary>Optional seed for the random number generator. <c>null</c> means non-deterministic.</summary>
    public int? Seed { get; init; }

    /// <summary>Event listeners to be notified after each throw.</summary>
    public IReadOnlyList<IThrowEventListener> EventListeners { get; init; } = [];
}
