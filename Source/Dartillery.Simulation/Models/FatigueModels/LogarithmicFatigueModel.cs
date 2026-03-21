using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.FatigueModels;

/// <summary>
/// Realistic logarithmic fatigue model.
/// Fast initial fatigue, then plateau effect (more realistic).
/// </summary>
internal sealed class LogarithmicFatigueModel : IFatigueModel
{
    private readonly double _growthRate;

    /// <summary>
    /// Creates a logarithmic fatigue model.
    /// </summary>
    /// <param name="growthRate">How quickly fatigue sets in (default: 0.01).</param>
    public LogarithmicFatigueModel(double growthRate = 0.01)
    {
        _growthRate = growthRate;
    }

    public double CalculateFatigue(SessionState state, PlayerProfile profile)
    {
        // Logarithmic curve: fatigue = maxFatigue * (1 - e^(-throwCount * growthRate))
        // Fast initial increase, then plateau
        double fatigue = profile.MaxFatigue * (1.0 - Math.Exp(-state.ThrowCount * _growthRate));
        return fatigue;
    }
}
