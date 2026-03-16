using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Simulators;

/// <summary>
/// Calculates throw deviation using a 2D Gaussian (normal) distribution.
/// Uses the Box-Muller transformation for accurate normal distribution sampling.
/// </summary>
internal sealed class GaussianDeviationCalculator : IDeviationCalculator
{
    private readonly IRandomProvider _randomProvider;

    /// <summary>
    /// Creates a new Gaussian deviation calculator.
    /// </summary>
    /// <param name="randomProvider">Random number provider for sampling.</param>
    public GaussianDeviationCalculator(IRandomProvider randomProvider)
    {
        ArgumentNullException.ThrowIfNull(randomProvider);
        _randomProvider = randomProvider;
    }

    /// <inheritdoc />
    /// <remarks>
    /// For Gaussian distribution, the precision parameter represents the standard deviation (σ).
    /// Generates two independent Gaussian random variables for X and Y components.
    /// Statistical properties:
    /// <list type="bullet">
    /// <item><description>~68% of throws land within 1σ of the aim point</description></item>
    /// <item><description>~95% of throws land within 2σ of the aim point</description></item>
    /// <item><description>~99.7% of throws land within 3σ of the aim point</description></item>
    /// </list>
    /// Recommended values: 0.02 (professional) to 0.08 (beginner).
    /// </remarks>
    public (double DX, double DY) CalculateDeviation(double precision)
    {
        double standardDeviation = precision;

        double uniformRandom1 = _randomProvider.NextDouble();
        double uniformRandom2 = _randomProvider.NextDouble();

        // Guard against uniformRandom1 = 0 (log(0) = -infinity)
        if (uniformRandom1 == 0.0)
            uniformRandom1 = double.Epsilon;

        // Box-Muller transformation produces two independent standard normal variables
        double radius = Math.Sqrt(-2.0 * Math.Log(uniformRandom1));
        double theta = 2.0 * Math.PI * uniformRandom2;

        double standardNormalX = radius * Math.Cos(theta);
        double standardNormalY = radius * Math.Sin(theta);

        double dx = standardNormalX * standardDeviation;
        double dy = standardNormalY * standardDeviation;

        return (dx, dy);
    }
}
