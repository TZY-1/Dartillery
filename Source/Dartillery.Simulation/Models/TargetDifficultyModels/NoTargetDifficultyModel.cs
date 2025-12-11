using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.TargetDifficultyModels;

/// <summary>
/// No-op target difficulty model for testing or simulations where all targets are equally difficult.
/// </summary>
public sealed class NoTargetDifficultyModel : ITargetDifficultyModel
{
    public double GetDifficultyModifier(Target target) => 1.0;
}
