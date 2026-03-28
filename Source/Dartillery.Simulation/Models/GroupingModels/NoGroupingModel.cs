using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.GroupingModels;

/// <summary>
/// No-op grouping model for testing or simulations without dart blocking effects.
/// </summary>
internal sealed class NoGroupingModel : IGroupingModel
{
    /// <inheritdoc/>
    public DeflectionResult ApplyDeflection(
        Point2D hitPoint,
        List<ThrowResult> previousThrowsInVisit)
        => DeflectionResult.None(hitPoint);
}
