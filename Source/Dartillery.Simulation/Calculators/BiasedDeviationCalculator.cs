using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Calculators;

/// <summary>
/// Decorator that adds systematic player bias to throw deviation.
/// First decorator in chain - applies player-specific constant offset.
/// </summary>
internal sealed class BiasedDeviationCalculator : IContextualDeviationCalculator
{
    private readonly IDeviationCalculator _baseCalculator;

    public BiasedDeviationCalculator(IDeviationCalculator baseCalculator)
    {
        _baseCalculator = baseCalculator ?? throw new ArgumentNullException(nameof(baseCalculator));
    }

    public (double DX, double DY) CalculateDeviation(PlayerProfile profile, ThrowContext context)
    {
        // Calculate base skill + session tremor
        double effectiveSkill = profile.BaseSkill + context.SessionTremor;

        // Get random deviation from base calculator
        var (dx, dy) = _baseCalculator.CalculatePolarDeviation(effectiveSkill);

        // Add systematic bias
        double biasedDx = dx + profile.SystematicBiasX;
        double biasedDy = dy + profile.SystematicBiasY;

        return (biasedDx, biasedDy);
    }
}
