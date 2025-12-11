using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Calculators;

/// <summary>
/// Decorator that applies pressure-based precision modification.
/// Scales deviation magnitude based on game pressure.
/// </summary>
internal sealed class PressureModifiedDeviationCalculator : IContextualDeviationCalculator
{
    private readonly IContextualDeviationCalculator _inner;

    public PressureModifiedDeviationCalculator(IContextualDeviationCalculator inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public (double DX, double DY) CalculateDeviation(PlayerProfile profile, ThrowContext context)
    {
        var (dx, dy) = _inner.CalculateDeviation(profile, context);

        // Apply pressure modifier
        // pressureModifier > 1.0 = more scatter (worse under pressure)
        // pressureModifier < 1.0 = less scatter (better when relaxed) - rare
        double modifier = context.PressureModifier;

        return (dx * modifier, dy * modifier);
    }
}
