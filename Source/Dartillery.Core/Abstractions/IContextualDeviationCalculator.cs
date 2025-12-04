using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Extended deviation calculator with full context support.
/// Enables dynamic modifiers based on player profile and throw context.
/// </summary>
public interface IContextualDeviationCalculator
{
    /// <summary>
    /// Calculates throw deviation with context awareness.
    /// Returns Cartesian offset (dx, dy) in meters.
    /// </summary>
    /// <param name="profile">Player profile with skill and bias information.</param>
    /// <param name="context">Throw context with session state and modifiers.</param>
    /// <returns>Deviation tuple: (DX = horizontal offset, DY = vertical offset)</returns>
    (double DX, double DY) CalculateDeviation(PlayerProfile profile, ThrowContext context);
}
