using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Models dart blocking/grouping effects.
/// Already thrown darts can physically deflect subsequent throws that land nearby.
/// </summary>
public interface IGroupingModel
{
    /// <summary>
    /// Applies post-hit deflection based on previously thrown darts in current visit.
    /// If the hit point is near a sticking dart, it gets pushed away (physically deflected).
    /// </summary>
    /// <param name="hitPoint">The calculated hit point before deflection.</param>
    /// <param name="previousThrowsInVisit">Darts already thrown in current visit (0-2 darts).</param>
    /// <returns>Result containing the deflected hit point and deflection metadata.</returns>
    DeflectionResult ApplyDeflection(
        Point2D hitPoint,
        List<ThrowResult> previousThrowsInVisit);
}

/// <summary>
/// Result of a deflection calculation.
/// </summary>
public sealed record DeflectionResult(
    Point2D HitPoint,
    bool WasDeflected,
    double DeflectionDistance,
    Point2D? PreDeflectionPoint,
    Point2D? DeflectedByPoint)
{
    /// <summary>Creates a result with no deflection.</summary>
    public static DeflectionResult None(Point2D hitPoint) => new(hitPoint, false, 0, null, null);
}
