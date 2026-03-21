using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.FatigueModels;

/// <summary>
/// No-op fatigue model for testing or simple simulations.
/// Always returns zero fatigue (no fatigue effect).
/// </summary>
internal sealed class NoFatigueModel : IFatigueModel
{
    public double CalculateFatigue(SessionState state, PlayerProfile profile) => 0.0;
}
