using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;
using Dartillery.Shared;

namespace Dartillery.Simulation.Geometry;

/// <summary>
/// Resolves dartboard coordinates to targets.
/// </summary>
internal sealed class TargetResolver : ITargetResolver
{
    private readonly ISectorResolver _sectorResolver;
    private readonly IRingResolver _ringResolver;

    /// <summary>
    /// Creates a target resolver with default implementations.
    /// </summary>
    public TargetResolver()
        : this(new SectorResolver(), new RingResolver())
    {
    }

    /// <summary>
    /// Creates a target resolver with custom implementations (for testing/extensibility).
    /// </summary>
    internal TargetResolver(ISectorResolver sectorResolver, IRingResolver ringResolver)
    {
        _sectorResolver = sectorResolver ?? throw new ArgumentNullException(nameof(sectorResolver));
        _ringResolver = ringResolver ?? throw new ArgumentNullException(nameof(ringResolver));
    }

    public Target? Resolve(Point2D point)
    {
        double radius = point.DistanceFromOrigin;

        // Miss - outside the board boundary
        if (radius > BoardDimensions.BoardRadius)
        {
            return null;
        }

        // Determine ring type
        SegmentType segmentType = _ringResolver.ResolveRing(radius);

        // Bull segments - special targets without sector numbers
        if (segmentType == SegmentType.InnerBull)
        {
            return Target.Bullseye();
        }

        if (segmentType == SegmentType.OuterBull)
        {
            return Target.OuterBull();
        }

        // Resolve sector for numbered segments
        int sectorNumber = _sectorResolver.ResolveSector(point);

        // Create target based on segment type
        return Target.Create(segmentType, sectorNumber);
    }
}
