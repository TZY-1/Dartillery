namespace Dartillery;

/// <summary>
/// Determines how throw deviations are distributed around the aim point.
/// </summary>
public enum SpreadMode
{
    /// <summary>Normal distribution (Bell curve) — most throws land near the target, rare outliers further out.</summary>
    Gaussian,

    /// <summary>Uniform distribution within a circle — every point within the spread radius is equally likely.</summary>
    Uniform,

    /// <summary>Bivariate Gaussian — elliptical spread with independent axis sigma, rotation, and throw-to-throw consistency variation.</summary>
    Bivariate
}
