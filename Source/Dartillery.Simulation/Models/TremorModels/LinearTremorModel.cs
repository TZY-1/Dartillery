using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.TremorModels;

/// <summary>
/// Simple linear tremor accumulation model.
/// Tremor increases linearly with throw count.
/// </summary>
internal sealed class LinearTremorModel : ITremorModel
{
    public double CalculateTremor(SessionState state, PlayerProfile profile)
    {
        double tremor = state.ThrowCount * profile.FatigueRate;
        return Math.Min(tremor, profile.MaxTremor);
    }
}
