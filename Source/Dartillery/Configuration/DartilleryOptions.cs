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

    /// <summary>
    /// Configures the simulator to use Gaussian distribution.
    /// </summary>
    public void UseGaussianDistribution() => DistributionType = DeviationDistribution.Gaussian;

    /// <summary>
    /// Configures the simulator to use uniform distribution.
    /// </summary>
    public void UseUniformDistribution() => DistributionType = DeviationDistribution.Uniform;

    /// <summary>
    /// Sets precision to professional level (sigma = 0.02).
    /// </summary>
    public void UseProfessionalPrecision() => StandardDeviation = 0.02;

    /// <summary>
    /// Sets precision to amateur level (sigma = 0.05).
    /// </summary>
    public void UseAmateurPrecision() => StandardDeviation = 0.05;

    /// <summary>
    /// Sets precision to beginner level (sigma = 0.08).
    /// </summary>
    public void UseBeginnerPrecision() => StandardDeviation = 0.08;
}
