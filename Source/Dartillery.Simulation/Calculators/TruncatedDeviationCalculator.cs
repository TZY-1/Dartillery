using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Calculators;

/// <summary>
/// Optional decorator that caps maximum deviation magnitude.
/// Prevents unrealistic outlier throws (e.g., beyond 3 standard deviations).
/// </summary>
internal sealed class TruncatedDeviationCalculator : IContextualDeviationCalculator
{
    private readonly IContextualDeviationCalculator _inner;
    private readonly double _maxDeviationMeters;

    public TruncatedDeviationCalculator(
        IContextualDeviationCalculator inner,
        double maxDeviationMeters = 0.25)
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
        _maxDeviationMeters = maxDeviationMeters;
    }

    public (double DX, double DY) CalculateDeviation(PlayerProfile profile, ThrowContext context)
    {
        var (dx, dy) = _inner.CalculateDeviation(profile, context);

        double magnitude = Math.Sqrt((dx * dx) + (dy * dy));

        // If deviation exceeds max, scale it down proportionally
        if (magnitude > _maxDeviationMeters)
        {
            double scale = _maxDeviationMeters / magnitude;
            return (dx * scale, dy * scale);
        }

        return (dx, dy);
    }
}
