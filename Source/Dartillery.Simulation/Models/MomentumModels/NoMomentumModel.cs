using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.MomentumModels;

/// <summary>
/// No-op momentum model for testing or simulations without momentum effects.
/// Always returns neutral momentum (1.0 = no effect).
/// </summary>
internal sealed class NoMomentumModel : IMomentumModel
{
    public double CalculateMomentumModifier(IReadOnlyList<ThrowResult> recentHistory) => 1.0;
}
