using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Models pressure/clutch effects on throw precision.
/// Returns multiplier: > 1.0 = more scatter (under pressure).
/// </summary>
public interface IPressureModel
{
    /// <summary>
    /// Calculates precision modifier based on game pressure.
    /// </summary>
    /// <param name="profile">Player profile with pressure resistance.</param>
    /// <param name="context">Game context with checkout state, remaining score, etc.</param>
    /// <returns>Precision multiplier (1.0 = neutral, > 1.0 = pressure increases deviation).</returns>
    double GetPrecisionModifier(PlayerProfile profile, GameContext context);
}
