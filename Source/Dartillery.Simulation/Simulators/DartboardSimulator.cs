using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Simulators;

/// <summary>
/// Main class for dart throw simulation.
/// Composes individual services (Deviation, Resolver, AimPoint).
/// </summary>
public sealed class DartboardSimulator : IThrowSimulator, IPointThrowSimulator
{
    private readonly IDeviationCalculator _deviationCalculator;
    private readonly ISegmentResolver _segmentResolver;
    private readonly IAimPointCalculator _aimPointCalculator;
    private readonly double _precision;

    /// <summary>
    /// Creates a new simulator with the specified dependencies.
    /// </summary>
    /// <param name="deviationCalculator">Calculator for throw deviation/scatter.</param>
    /// <param name="segmentResolver">Resolver for mapping hit points to board segments.</param>
    /// <param name="aimPointCalculator">Calculator for target aim points.</param>
    /// <param name="precision">Precision parameter (lower = more accurate). Default: 0.03</param>
    public DartboardSimulator(
        IDeviationCalculator deviationCalculator,
        ISegmentResolver segmentResolver,
        IAimPointCalculator aimPointCalculator,
        double precision = 0.03)
    {
        _deviationCalculator = deviationCalculator ?? throw new ArgumentNullException(nameof(deviationCalculator));
        _segmentResolver = segmentResolver ?? throw new ArgumentNullException(nameof(segmentResolver));
        _aimPointCalculator = aimPointCalculator ?? throw new ArgumentNullException(nameof(aimPointCalculator));

        ValidatePrecision(precision);
        _precision = precision;
    }

    /// <inheritdoc />
    public ThrowResult Throw(Target target)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        Point2D aimPoint = _aimPointCalculator.CalculateAimPoint(target);
        return ThrowAt(aimPoint);
    }

    /// <inheritdoc />
    public ThrowResult ThrowAt(Point2D aimPoint)
    {
        // Get Cartesian deviation (dx, dy) - works in all 360° directions
        var (dx, dy) = _deviationCalculator.CalculatePolarDeviation(_precision);

        // Add deviation directly to aim point
        Point2D hitPoint = new(aimPoint.X + dx, aimPoint.Y + dy);

        return _segmentResolver.Resolve(hitPoint, aimPoint);
    }

    /// <summary>
    /// Validates that the precision parameter is valid.
    /// </summary>
    private static void ValidatePrecision(double precision)
    {
        if (double.IsNaN(precision))
            throw new ArgumentException("Precision cannot be NaN.", nameof(precision));
        if (double.IsInfinity(precision))
            throw new ArgumentException("Precision cannot be infinite.", nameof(precision));
        if (precision < 0)
            throw new ArgumentOutOfRangeException(nameof(precision),
                "Precision must be non-negative.");
        if (precision == 0)
            throw new ArgumentOutOfRangeException(nameof(precision),
                "Precision must be greater than zero for meaningful simulation.");
    }
}
