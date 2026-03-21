using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Models fatigue accumulation during a session.
/// Different implementations provide different fatigue curves.
/// </summary>
public interface IFatigueModel
{
    /// <summary>
    /// Calculates current fatigue magnitude based on session state.
    /// Fatigue is added to base skill to simulate declining accuracy.
    /// </summary>
    /// <param name="state">Current session state (throw count, duration, etc.).</param>
    /// <param name="profile">Player profile with fatigue rate and limits.</param>
    /// <returns>Fatigue magnitude to add to base skill (always >= 0).</returns>
    double CalculateFatigue(SessionState state, PlayerProfile profile);
}
