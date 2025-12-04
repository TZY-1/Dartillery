using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Interface for resolving coordinates to dartboard segments.
/// </summary>
public interface ISegmentResolver
{
    /// <summary>
    /// Resolves a point on the board to a throw result.
    /// </summary>
    /// <param name="hitPoint">The point where the dart hit.</param>
    /// <param name="aimedPoint">The original aim point.</param>
    /// <returns>The throw result with segment information and score.</returns>
    ThrowResult Resolve(Point2D hitPoint, Point2D aimedPoint);
}
