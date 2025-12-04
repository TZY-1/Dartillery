namespace Dartillery.Core.Abstractions;

/// <summary>
/// Interface for random number generation.
/// Enables dependency injection and improved testability.
/// </summary>
public interface IRandomProvider
{
    /// <summary>
    /// Returns a random floating-point number between 0.0 and 1.0.
    /// </summary>
    double NextDouble();
}
