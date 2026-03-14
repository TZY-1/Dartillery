using Dartillery.Core.Abstractions;

namespace Dartillery;

/// <summary>
/// Configuration options for Dartillery simulation services.
/// </summary>
public sealed class DartilleryOptions
{
    /// <summary>
    /// The type of deviation distribution to use.
    /// Default is Gaussian (most realistic).
    /// </summary>
    public DeviationDistribution DistributionType { get; set; } = DeviationDistribution.Gaussian;

    /// <summary>
    /// Standard deviation for throw precision.
    /// Lower values = more precise throws.
    /// Typical range: 0.02 (professional) to 0.08 (beginner).
    /// Default: 0.03
    /// </summary>
    public double StandardDeviation { get; set; } = 0.03;

    /// <summary>
    /// Optional seed for reproducible random number generation.
    /// If null, a random seed will be used.
    /// </summary>
    public int? Seed { get; set; }

    /// <summary>
    /// Custom deviation calculator (only used when DistributionType is Custom).
    /// </summary>
    internal IDeviationCalculator? CustomDeviationCalculator { get; set; }
}
