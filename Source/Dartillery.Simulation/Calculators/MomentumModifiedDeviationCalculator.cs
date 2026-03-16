using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Calculators;

/// <summary>
/// Decorator that applies momentum (hot hand / cold streak) effects.
/// </summary>
internal sealed class MomentumModifiedDeviationCalculator : IContextualDeviationCalculator
{
    private readonly IContextualDeviationCalculator _inner;

    public MomentumModifiedDeviationCalculator(IContextualDeviationCalculator inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
    }

    public (double DX, double DY) CalculateDeviation(PlayerProfile profile, ThrowContext context)
    {
        var (dx, dy) = _inner.CalculateDeviation(profile, context);

        // momentumModifier < 1.0 = hot hand (less deviation)
        // momentumModifier > 1.0 = cold streak (more deviation)
        double modifier = context.MomentumModifier;

        return (dx * modifier, dy * modifier);
    }
}
