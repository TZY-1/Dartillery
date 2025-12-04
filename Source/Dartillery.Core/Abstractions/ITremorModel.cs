using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Models fatigue-based tremor accumulation during a session.
/// Different implementations provide different fatigue curves.
/// </summary>
public interface ITremorModel
{
    /// <summary>
    /// Calculates current tremor magnitude based on session state.
    /// Tremor is added to base skill to simulate declining accuracy.
    /// </summary>
    /// <param name="state">Current session state (throw count, duration, etc.).</param>
    /// <param name="profile">Player profile with fatigue rate and limits.</param>
    /// <returns>Tremor magnitude to add to base skill (always >= 0).</returns>
    double CalculateTremor(SessionState state, PlayerProfile profile);
}
