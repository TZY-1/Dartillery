using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.TremorModels;

/// <summary>
/// No-op tremor model for testing or simple simulations.
/// Always returns zero tremor (no fatigue effect).
/// </summary>
public sealed class NoTremorModel : ITremorModel
{
    public double CalculateTremor(SessionState state, PlayerProfile profile) => 0.0;
}
