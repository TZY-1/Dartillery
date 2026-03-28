using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Calculators;

/// <summary>
/// Optional decorator that caps deviation to within the spread algorithm's bounds.
/// Shape-agnostic — works for circles, ellipses, and any future boundary type.
/// </summary>
internal sealed class TruncatedDeviationCalculator : IContextualDeviationCalculator
{
    private readonly IContextualDeviationCalculator _inner;
    private readonly ISpreadBounds _bounds;

    /// <summary>Initializes a truncation decorator with the given inner calculator and bounds.</summary>
    public TruncatedDeviationCalculator(
        IContextualDeviationCalculator inner,
        ISpreadBounds bounds)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(bounds);
        _inner = inner;
        _bounds = bounds;
    }

    /// <inheritdoc/>
    public (double DX, double DY) CalculateDeviation(PlayerProfile profile, ThrowContext context)
    {
        var (dx, dy) = _inner.CalculateDeviation(profile, context);

        if (!_bounds.Contains(dx, dy))
        {
            // Scale down proportionally to land on the boundary
            double distance = Math.Sqrt((dx * dx) + (dy * dy));
            if (distance > 0)
            {
                // Use NormalizedDistance to find the scale factor
                double normalized = _bounds.NormalizedDistance(dx, dy);
                if (normalized > 0)
                {
                    double scale = 1.0 / normalized;
                    return (dx * scale, dy * scale);
                }
            }
        }

        return (dx, dy);
    }
}
