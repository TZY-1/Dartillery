using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.PressureModels;

/// <summary>
/// No-op pressure model for testing or pressure-free simulations.
/// Always returns neutral pressure (1.0 = no effect).
/// </summary>
public sealed class NoPressureModel : IPressureModel
{
    public double GetPrecisionModifier(PlayerProfile profile, GameContext context) => 1.0;
}
