using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Interface for calculating aim points on the dartboard.
/// </summary>
public interface IAimPointCalculator
{
    /// <summary>
    /// Calculates the optimal aim point for a target.
    /// </summary>
    /// <param name="target">The desired target.</param>
    /// <returns>The aim point in board coordinates.</returns>
    Point2D CalculateAimPoint(Target target);
}
