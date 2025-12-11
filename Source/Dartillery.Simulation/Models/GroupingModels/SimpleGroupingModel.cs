using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.GroupingModels;

/// <summary>
/// Simple dart grouping model.
/// If 2+ darts are close to aim point, increase deviation for next dart (blocking effect).
/// </summary>
public sealed class SimpleGroupingModel : IGroupingModel
{
    private readonly double _clusterRadius;
    private readonly double _blockingPenalty;

    /// <summary>
    /// Creates a simple grouping model.
    /// </summary>
    /// <param name="clusterRadius">Radius to consider darts "clustered" (default: 0.03m = 3cm).</param>
    /// <param name="blockingPenalty">Deviation increase when blocking occurs (default: 0.15 = 15% worse).</param>
    public SimpleGroupingModel(
        double clusterRadius = 0.03,
        double blockingPenalty = 0.15)
    {
        _clusterRadius = clusterRadius;
        _blockingPenalty = blockingPenalty;
    }

    public (Point2D AdjustedAimPoint, double DeviationMultiplier) AdjustForGrouping(
        Point2D originalAimPoint,
        List<ThrowResult> previousThrowsInVisit)
    {
        if (previousThrowsInVisit.Count < 2)
        {
            return (originalAimPoint, 1.0); // No grouping effect with 0-1 darts
        }

        // Count darts near aim point
        int nearbyDarts = previousThrowsInVisit
            .Count(r => r.HitPoint.DistanceTo(originalAimPoint) < _clusterRadius);

        if (nearbyDarts >= 2)
        {
            // Blocking effect: increase deviation
            return (originalAimPoint, 1.0 + _blockingPenalty);
        }

        return (originalAimPoint, 1.0);
    }
}
