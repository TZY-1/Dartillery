using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Models dart blocking/grouping effects.
/// Already thrown darts can obstruct or deflect subsequent throws.
/// </summary>
public interface IGroupingModel
{
    /// <summary>
    /// Adjusts aim point based on previously thrown darts in current visit.
    /// May shift aim point or increase deviation if darts are clustered.
    /// </summary>
    /// <param name="originalAimPoint">Original target point.</param>
    /// <param name="previousThrowsInVisit">Darts already thrown in current visit (0-2 darts).</param>
    /// <returns>Tuple of (adjusted aim point, deviation multiplier for blocking effect).</returns>
    (Point2D AdjustedAimPoint, double DeviationMultiplier) AdjustForGrouping(
        Point2D originalAimPoint,
        List<ThrowResult> previousThrowsInVisit);
}
