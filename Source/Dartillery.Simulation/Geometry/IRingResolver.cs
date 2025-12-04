using Dartillery.Core.Enums;

namespace Dartillery.Simulation.Geometry;

/// <summary>
/// Resolves dartboard radius to ring segment type.
/// </summary>
internal interface IRingResolver
{
    /// <summary>
    /// Determines the ring type based on distance from center.
    /// </summary>
    /// <param name="radius">Distance from board center in normalized units (0-1).</param>
    /// <returns>The segment type (Single, Double, Triple, InnerBull, OuterBull).</returns>
    SegmentType ResolveRing(double radius);
}
