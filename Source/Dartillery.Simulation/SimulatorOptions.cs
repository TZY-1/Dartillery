namespace Dartillery.Simulation.Configuration;

/// <summary>
/// Konfigurationsoptionen für die Wurf-Simulation.
/// </summary>
public sealed class SimulatorOptions
{
    /// <summary>
    /// Standardabweichung der Streuung.
    /// Kleinere Werte = präzisere Würfe.
    /// Typische Werte: 0.02 (Profi) bis 0.08 (Anfänger).
    /// </summary>
    public double Sigma { get; set; } = 0.03;

    /// <summary>
    /// Seed für den Zufallsgenerator. Null = zufälliger Seed.
    /// </summary>
    public int? Seed { get; set; }

    /// <summary>
    /// Standard-Optionen mit moderater Streuung.
    /// </summary>
    public static SimulatorOptions Default => new();

    /// <summary>
    /// Optionen für einen Profi-Spieler (geringe Streuung).
    /// </summary>
    public static SimulatorOptions Professional => new() { Sigma = 0.02 };

    /// <summary>
    /// Optionen für einen Amateur (mittlere Streuung).
    /// </summary>
    public static SimulatorOptions Amateur => new() { Sigma = 0.05 };

    /// <summary>
    /// Optionen für einen Anfänger (hohe Streuung).
    /// </summary>
    public static SimulatorOptions Beginner => new() { Sigma = 0.08 };
}