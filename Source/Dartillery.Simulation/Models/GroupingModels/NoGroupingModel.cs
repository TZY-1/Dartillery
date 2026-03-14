using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.GroupingModels;

/// <summary>
/// No-op grouping model for testing or simulations without dart blocking effects.
/// </summary>
internal sealed class NoGroupingModel : IGroupingModel
{
    public (Point2D AdjustedAimPoint, double DeviationMultiplier) AdjustForGrouping(
        Point2D originalAimPoint,
        List<ThrowResult> previousThrowsInVisit)
        => (originalAimPoint, 1.0);
}
