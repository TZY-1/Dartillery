namespace Dartillery;

/// <summary>
/// Types of statistical distributions for throw deviation.
/// </summary>
public enum DeviationDistribution
{
    /// <summary>
    /// Gaussian (normal) distribution - most realistic for dart throws.
    /// </summary>
    Gaussian,

    /// <summary>
    /// Uniform circular distribution - all points within radius equally likely.
    /// </summary>
    Uniform,

    /// <summary>
    /// Custom distribution using a user-provided IDeviationCalculator.
    /// </summary>
    Custom
}
