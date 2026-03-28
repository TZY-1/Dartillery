using Dartillery.Core.Enums;
using Dartillery.Shared;

namespace Dartillery.Simulation.Geometry;

/// <summary>
/// Default implementation of ring resolution using standard dartboard dimensions.
/// </summary>
internal sealed class RingResolver : IRingResolver
{
    /// <inheritdoc/>
    public SegmentType ResolveRing(double radius)
    {
        // Handle bull regions (no sector dependency)
        if (radius <= BoardDimensions.InnerBullRadius)
            return SegmentType.InnerBull;

        if (radius <= BoardDimensions.OuterBullRadius)
            return SegmentType.OuterBull;

        // Handle scoring rings
        if (radius >= BoardDimensions.TripleRingInner &&
            radius <= BoardDimensions.TripleRingOuter)
        {
            return SegmentType.Triple;
        }

        if (radius >= BoardDimensions.DoubleRingInner &&
            radius <= BoardDimensions.DoubleRingOuter)
        {
            return SegmentType.Double;
        }

        // Everything else is Single
        return SegmentType.Single;
    }
}
