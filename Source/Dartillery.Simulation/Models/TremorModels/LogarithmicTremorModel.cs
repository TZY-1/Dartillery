using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.TremorModels;

/// <summary>
/// Realistic logarithmic tremor model.
/// Fast initial fatigue, then plateau effect (more realistic).
/// </summary>
internal sealed class LogarithmicTremorModel : ITremorModel
{
    private readonly double _growthRate;

    /// <summary>
    /// Creates a logarithmic tremor model.
    /// </summary>
    /// <param name="growthRate">How quickly fatigue sets in (default: 0.01).</param>
    public LogarithmicTremorModel(double growthRate = 0.01)
    {
        _growthRate = growthRate;
    }

    public double CalculateTremor(SessionState state, PlayerProfile profile)
    {
        // Logarithmic curve: tremor = maxTremor * (1 - e^(-throwCount * growthRate))
        // Fast initial increase, then plateau
        double tremor = profile.MaxTremor * (1.0 - Math.Exp(-state.ThrowCount * _growthRate));
        return tremor;
    }
}
