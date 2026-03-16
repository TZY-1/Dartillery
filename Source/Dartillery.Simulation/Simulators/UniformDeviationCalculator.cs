using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Simulators;

/// <summary>
/// Calculates throw deviation using a uniform circular distribution.
/// All points within the circle have equal probability.
/// </summary>
internal sealed class UniformDeviationCalculator : IDeviationCalculator
{
    private readonly IRandomProvider _randomProvider;

    /// <summary>
    /// Creates a new uniform deviation calculator.
    /// </summary>
    /// <param name="randomProvider">Random number provider for sampling.</param>
    public UniformDeviationCalculator(IRandomProvider randomProvider)
    {
        ArgumentNullException.ThrowIfNull(randomProvider);
        _randomProvider = randomProvider;
    }

    /// <inheritdoc />
    /// <remarks>
    /// For uniform distribution, the precision parameter represents the maximum radius.
    /// All throws land uniformly distributed within a circle of this radius from the aim point.
    /// To match Gaussian ~95% accuracy, use: uniformRadius = gaussianSigma * 2.0
    /// </remarks>
    public (double DX, double DY) CalculateDeviation(double precision)
    {
        double maxRadius = precision;

        // Uniform distribution within a circle requires sqrt(uniform) for radius to avoid
        // clustering at center — see area-preserving polar coordinate sampling
        double angle = 2.0 * Math.PI * _randomProvider.NextDouble();
        double radius = maxRadius * Math.Sqrt(_randomProvider.NextDouble());

        double dx = radius * Math.Cos(angle);
        double dy = radius * Math.Sin(angle);

        return (dx, dy);
    }
}
