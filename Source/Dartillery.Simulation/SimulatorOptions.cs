namespace Dartillery.Simulation.Configuration;

/// <summary>
/// Configuration options for the throw simulation.
/// </summary>
public sealed class SimulatorOptions
{
    /// <summary>
    /// Standard deviation of the throw dispersion.
    /// Smaller values = more precise throws.
    /// </summary>
    public double Sigma { get; set; } = 0.03;

    /// <summary>
    /// Seed for the random generator. Null = random seed.
    /// </summary>
    public int? Seed { get; set; }

    /// <summary>
    /// Default options with moderate dispersion.
    /// </summary>
    public static SimulatorOptions Default => new();

    /// <summary>
    /// Options for a professional player (low dispersion).
    /// </summary>
    public static SimulatorOptions Professional => new() { Sigma = 0.02 };

    /// <summary>
    /// Options for an amateur (medium dispersion).
    /// </summary>
    public static SimulatorOptions Amateur => new() { Sigma = 0.05 };

    /// <summary>
    /// Options for a beginner (high dispersion).
    /// </summary>
    public static SimulatorOptions Beginner => new() { Sigma = 0.08 };
}