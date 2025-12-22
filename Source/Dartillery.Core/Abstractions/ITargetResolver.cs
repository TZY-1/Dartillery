using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Interface for resolving coordinates to dartboard targets.
/// </summary>
public interface ITargetResolver
{
    /// <summary>
    /// Resolves a point on the board to a target.
    /// </summary>
    /// <param name="point">The point on the dartboard in Cartesian coordinates.</param>
    /// <returns>The target at the given point, or null if the point is outside the board (miss).</returns>
    Target? Resolve(Point2D point);
}
