using System.Linq;
using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.GroupingModels;

/// <summary>
/// Distance-based dart deflection model.
/// If a dart lands near a sticking dart, it gets pushed away proportionally to how close it is.
/// </summary>
internal sealed class SimpleGroupingModel : IGroupingModel
{
    private readonly double _clusterRadius;
    private readonly double _maxDeflection;

    /// <summary>
    /// Creates a simple grouping model with distance-based deflection.
    /// </summary>
    /// <param name="clusterRadius">Normalized radius within which blocking occurs (default: 0.08 ≈ 14mm).</param>
    /// <param name="maxDeflection">Maximum deflection offset when darts overlap exactly (default: 0.04 ≈ 7mm).</param>
    public SimpleGroupingModel(
        double clusterRadius = 0.08,
        double maxDeflection = 0.04)
    {
        _clusterRadius = clusterRadius;
        _maxDeflection = maxDeflection;
    }

    /// <inheritdoc/>
    public DeflectionResult ApplyDeflection(
        Point2D hitPoint,
        List<ThrowResult> previousThrowsInVisit)
    {
        if (previousThrowsInVisit.Count == 0)
            return DeflectionResult.None(hitPoint);

        double currentX = hitPoint.X;
        double currentY = hitPoint.Y;
        Point2D? closestBlocker = null;
        double closestDist = double.MaxValue;

        foreach (var prevHitPoint in previousThrowsInVisit.Select(prev => prev.HitPoint))
        {
            double dx = currentX - prevHitPoint.X;
            double dy = currentY - prevHitPoint.Y;
            double dist = Math.Sqrt((dx * dx) + (dy * dy));

            if (dist < _clusterRadius)
            {
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestBlocker = prevHitPoint;
                }

                double factor = 1.0 - (dist / _clusterRadius);
                double offset = _maxDeflection * factor;

                if (dist < 0.0001)
                {
                    double centerDist = Math.Sqrt((currentX * currentX) + (currentY * currentY));
                    if (centerDist < 0.0001)
                    {
                        currentX += offset;
                    }
                    else
                    {
                        currentX += (currentX / centerDist) * offset;
                        currentY += (currentY / centerDist) * offset;
                    }
                }
                else
                {
                    currentX += (dx / dist) * offset;
                    currentY += (dy / dist) * offset;
                }
            }
        }

        if (closestBlocker == null)
            return DeflectionResult.None(hitPoint);

        var deflectedPoint = new Point2D(currentX, currentY);
        double deflectionDist = hitPoint.DistanceTo(deflectedPoint);

        return new DeflectionResult(
            deflectedPoint,
            true,
            deflectionDist,
            hitPoint,
            closestBlocker);
    }
}
