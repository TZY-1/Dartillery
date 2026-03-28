using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.FatigueModels;

/// <summary>
/// Fatigue model with recovery between throws.
/// Fatigue decreases during pauses (between visits).
/// Decorator pattern - wraps another fatigue model.
/// </summary>
internal sealed class RecoveryFatigueModel : IFatigueModel
{
    private readonly IFatigueModel _baseFatigueModel;
    private readonly double _recoveryRate;

    /// <summary>
    /// Creates a recovery fatigue model.
    /// </summary>
    /// <param name="baseFatigueModel">Base fatigue model to wrap.</param>
    /// <param name="recoveryRate">Recovery rate per second of pause (default: 0.1).</param>
    public RecoveryFatigueModel(IFatigueModel baseFatigueModel, double recoveryRate = 0.1)
    {
        ArgumentNullException.ThrowIfNull(baseFatigueModel);
        _baseFatigueModel = baseFatigueModel;
        _recoveryRate = recoveryRate;
    }

    /// <inheritdoc/>
    public double CalculateFatigue(SessionState state, PlayerProfile profile)
    {
        double baseFatigue = _baseFatigueModel.CalculateFatigue(state, profile);

        double recoverySeconds = state.TimeSinceLastThrow.TotalSeconds;
        double recovery = recoverySeconds * _recoveryRate * profile.FatigueRate;

        double recoveredFatigue = Math.Max(0, state.CurrentFatigue - recovery);

        // Take the higher of recovered fatigue and accumulated fatigue — fatigue can still increase mid-session
        return Math.Max(recoveredFatigue, baseFatigue);
    }
}
