using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Interface for calculating random deviations from an aim point for dart throw simulation.
/// </summary>
public interface IDeviationCalculator
{
    /// <summary>
    /// Calculates throw deviation as a Cartesian offset (dx, dy).
    /// Returns independent X and Y deviations that work in all 360° directions.
    /// </summary>
    /// <param name="precision">
    /// Precision parameter controlling throw accuracy. Lower values produce more precise throws.
    /// The exact interpretation depends on the implementation:
    /// <list type="bullet">
    /// <item><description>Gaussian: Standard deviation (σ) for both X and Y components</description></item>
    /// <item><description>Uniform: Maximum radius of the circular distribution</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// Cartesian deviation tuple:
    /// <list type="bullet">
    /// <item><description>DX: Horizontal deviation (positive = right, negative = left)</description></item>
    /// <item><description>DY: Vertical deviation (positive = up, negative = down)</description></item>
    /// </list>
    /// </returns>
    (double DX, double DY) CalculateDeviation(double precision);
}
