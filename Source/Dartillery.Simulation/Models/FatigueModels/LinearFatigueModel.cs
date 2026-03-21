using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.FatigueModels;

/// <summary>
/// Simple linear fatigue accumulation model.
/// Fatigue increases linearly with throw count.
/// </summary>
internal sealed class LinearFatigueModel : IFatigueModel
{
    public double CalculateFatigue(SessionState state, PlayerProfile profile)
    {
        double fatigue = state.ThrowCount * profile.FatigueRate;
        return Math.Min(fatigue, profile.MaxFatigue);
    }
}
