namespace Dartillery.Core.Utilities;

/// <summary>
/// Utility methods for converting between different precision parameter interpretations.
/// </summary>
public static class PrecisionConverter
{
    /// <summary>
    /// Converts uniform distribution max radius to equivalent Gaussian standard deviation.
    /// Equivalence is based on the 95% containment circle (2σ for Gaussian).
    /// </summary>
    /// <param name="uniformMaxRadius">The maximum radius for uniform distribution.</param>
    /// <returns>Equivalent Gaussian standard deviation.</returns>
    public static double UniformToGaussian(double uniformMaxRadius)
    {
        return uniformMaxRadius / 2.0;
    }

    /// <summary>
    /// Converts Gaussian standard deviation to equivalent uniform max radius.
    /// Equivalence is based on the 95% containment circle (2σ for Gaussian).
    /// </summary>
    /// <param name="gaussianStandardDeviation">The Gaussian standard deviation.</param>
    /// <returns>Equivalent uniform maximum radius.</returns>
    public static double GaussianToUniform(double gaussianStandardDeviation)
    {
        return gaussianStandardDeviation * 2.0;
    }
}
