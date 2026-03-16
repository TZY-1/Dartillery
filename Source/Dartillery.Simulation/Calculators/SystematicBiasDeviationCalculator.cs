using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Calculators;

/// <summary>
/// Decorator that adds systematic player bias to throw deviation.
/// First decorator in chain - applies player-specific constant offset.
/// </summary>
internal sealed class SystematicBiasDeviationCalculator : IContextualDeviationCalculator
{
    private readonly IDeviationCalculator _baseCalculator;

    public SystematicBiasDeviationCalculator(IDeviationCalculator baseCalculator)
    {
        ArgumentNullException.ThrowIfNull(baseCalculator);
        _baseCalculator = baseCalculator;
    }

    public (double DX, double DY) CalculateDeviation(PlayerProfile profile, ThrowContext context)
    {
        double effectiveSkill = profile.BaseSkill + context.SessionTremor;
        var (dx, dy) = _baseCalculator.CalculateDeviation(effectiveSkill);
        double biasedDx = dx + profile.SystematicBiasX;
        double biasedDy = dy + profile.SystematicBiasY;

        return (biasedDx, biasedDy);
    }
}
