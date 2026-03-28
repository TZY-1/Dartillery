using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.PressureModels;

/// <summary>
/// No-op pressure model for testing or pressure-free simulations.
/// Always returns neutral pressure (1.0 = no effect).
/// </summary>
internal sealed class NoPressureModel : IPressureModel
{
    /// <inheritdoc/>
    public double GetPrecisionModifier(PlayerProfile profile, GameContext context) => 1.0;
}
