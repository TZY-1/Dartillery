using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.TargetDifficultyModels;

/// <summary>
/// Standard target difficulty model.
/// Bulls are harder than triples, doubles harder than singles (smaller targets).
/// </summary>
internal sealed class StandardTargetDifficultyModel : ITargetDifficultyModel
{
    public double GetDifficultyModifier(Target target)
    {
        return target.SegmentType switch
        {
            SegmentType.InnerBull => 1.3,   // Hardest (smallest target)
            SegmentType.OuterBull => 1.15,  // Hard
            SegmentType.Triple => 1.0,      // Standard baseline
            SegmentType.Double => 1.05,     // Slightly harder (edge of board)
            SegmentType.Single => 0.95,     // Easiest (largest area)
            _ => 1.0
        };
    }
}
