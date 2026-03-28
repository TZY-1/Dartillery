using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Simulators;

/// <summary>
/// Calculates throw deviation using a bivariate Gaussian distribution with independent
/// horizontal/vertical sigma, rotation, and per-throw consistency variation.
/// Models realistic human throwing patterns where accuracy differs by axis.
/// </summary>
internal sealed class BivariateDeviationCalculator : IDeviationCalculator
{
    private const double _degToRad = Math.PI / 180.0;

    private readonly IRandomProvider _randomProvider;
    private readonly double _sigmaRatio;
    private readonly double _angleDegrees;
    private readonly double _consistency;

    /// <summary>
    /// Creates a new bivariate deviation calculator.
    /// </summary>
    /// <param name="randomProvider">Random number provider for sampling.</param>
    /// <param name="sigmaRatio">Ratio of vertical to horizontal sigma (0.3–1.0). At 1.0, degenerates to circular Gaussian.</param>
    /// <param name="angleDegrees">Rotation angle in degrees (-90 to +90). Rotates the spread ellipse.</param>
    /// <param name="consistency">Throw-to-throw consistency (0.0–1.0). Lower values introduce more per-throw sigma variation.</param>
    public BivariateDeviationCalculator(
        IRandomProvider randomProvider,
        double sigmaRatio = 0.7,
        double angleDegrees = 0.0,
        double consistency = 0.8)
    {
        ArgumentNullException.ThrowIfNull(randomProvider);
        _randomProvider = randomProvider;
        _sigmaRatio = Math.Clamp(sigmaRatio, 0.3, 1.0);
        _angleDegrees = Math.Clamp(angleDegrees, -90.0, 90.0);
        _consistency = Math.Clamp(consistency, 0.0, 1.0);
    }

    /// <inheritdoc />
    /// <remarks>
    /// The precision parameter is the base sigma (modified by fatigue etc. before reaching here).
    /// The algorithm:
    /// 1. Apply per-throw consistency jitter to the base sigma
    /// 2. Compute independent sigmaX and sigmaY using the sigma ratio
    /// 3. Generate two independent standard normals via Box-Muller
    /// 4. Scale by respective sigma values
    /// 5. Rotate by the configured angle
    /// </remarks>
    public (double DX, double DY) CalculateDeviation(double precision)
    {
        // Per-throw consistency variation: jitter range scales with (1 - consistency)
        double jitterRange = (1.0 - _consistency) * 0.5;
        double jitter = jitterRange > 0
            ? ((_randomProvider.NextDouble() * 2.0) - 1.0) * jitterRange
            : 0.0;
        double effectiveSigma = precision * (1.0 + jitter);

        double sigmaX = effectiveSigma;
        double sigmaY = effectiveSigma * _sigmaRatio;

        // Box-Muller transformation for two independent standard normals
        double u1 = _randomProvider.NextDouble();
        double u2 = _randomProvider.NextDouble();
        if (u1 == 0.0) u1 = double.Epsilon;

        double radius = Math.Sqrt(-2.0 * Math.Log(u1));
        double theta = 2.0 * Math.PI * u2;

        double localDx = radius * Math.Cos(theta) * sigmaX;
        double localDy = radius * Math.Sin(theta) * sigmaY;

        // Apply rotation
        if (_angleDegrees != 0.0)
        {
            double rad = _angleDegrees * _degToRad;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            double rotatedDx = (localDx * cos) - (localDy * sin);
            double rotatedDy = (localDx * sin) + (localDy * cos);
            return (rotatedDx, rotatedDy);
        }

        return (localDx, localDy);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Returns elliptical bounds at the 3-sigma boundary (99.7% coverage).
    /// Bounds use nominal sigma (no consistency jitter) for stable visualization.
    /// </remarks>
    public ISpreadBounds GetBounds(double precision)
        => new EllipseBounds(precision * 3, precision * _sigmaRatio * 3, _angleDegrees);
}
