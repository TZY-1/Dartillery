using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.TremorModels;

/// <summary>
/// Tremor model with recovery between throws.
/// Tremor decreases during pauses (between visits).
/// Decorator pattern - wraps another tremor model.
/// </summary>
public sealed class RecoveryTremorModel : ITremorModel
{
    private readonly ITremorModel _baseTremorModel;
    private readonly double _recoveryRate;

    /// <summary>
    /// Creates a recovery tremor model.
    /// </summary>
    /// <param name="baseTremorModel">Base tremor model to wrap.</param>
    /// <param name="recoveryRate">Recovery rate per second of pause (default: 0.1).</param>
    public RecoveryTremorModel(ITremorModel baseTremorModel, double recoveryRate = 0.1)
    {
        _baseTremorModel = baseTremorModel ?? throw new ArgumentNullException(nameof(baseTremorModel));
        _recoveryRate = recoveryRate;
    }

    public double CalculateTremor(SessionState state, PlayerProfile profile)
    {
        // Calculate base tremor
        double baseTremor = _baseTremorModel.CalculateTremor(state, profile);

        // Apply recovery based on time since last throw
        double recoverySeconds = state.TimeSinceLastThrow.TotalSeconds;
        double recovery = recoverySeconds * _recoveryRate * profile.FatigueRate;

        // Current tremor with recovery applied
        double recoveredTremor = Math.Max(0, state.CurrentTremor - recovery);

        // Return max of recovered tremor and base tremor (fatigue can still increase)
        return Math.Max(recoveredTremor, baseTremor);
    }
}
