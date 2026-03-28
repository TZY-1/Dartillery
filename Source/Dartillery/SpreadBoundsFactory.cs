using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;
using Dartillery.Simulation.Services;
using Dartillery.Simulation.Simulators;

namespace Dartillery;

/// <summary>
/// Creates <see cref="ISpreadBounds"/> for a given spread mode and precision,
/// delegating to the actual calculator implementations so the radius formula
/// lives in exactly one place.
/// </summary>
public static class SpreadBoundsFactory
{
    /// <summary>
    /// Returns the spread bounds for the given mode and precision value.
    /// </summary>
    public static ISpreadBounds Create(SpreadMode mode, double precision)
        => Create(mode, precision, 1.0, 0.0);

    /// <summary>
    /// Returns the spread bounds for the given mode, precision, and bivariate parameters.
    /// The <paramref name="sigmaRatio"/> and <paramref name="angleDegrees"/> are only used for <see cref="SpreadMode.Bivariate"/>.
    /// </summary>
    public static ISpreadBounds Create(SpreadMode mode, double precision,
        double sigmaRatio, double angleDegrees)
    {
        // Use a throwaway RNG — GetBounds is deterministic, only uses the precision value.
        IDeviationCalculator calc = mode switch
        {
            SpreadMode.Uniform => new UniformDeviationCalculator(new DefaultRandomProvider()),
            SpreadMode.Bivariate => new BivariateDeviationCalculator(
                new DefaultRandomProvider(), sigmaRatio, angleDegrees),
            _ => new GaussianDeviationCalculator(new DefaultRandomProvider())
        };
        return calc.GetBounds(precision);
    }
}
