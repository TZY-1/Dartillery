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
        ArgumentNullException.ThrowIfNull(sectorResolver);
        ArgumentNullException.ThrowIfNull(ringResolver);
        _sectorResolver = sectorResolver;
        _ringResolver = ringResolver;
    }

    /// <inheritdoc/>
    public Target? Resolve(Point2D point)
    {
        double radius = point.DistanceFromOrigin;

        if (radius > BoardDimensions.BoardRadius)
            return null;

        SegmentType segmentType = _ringResolver.ResolveRing(radius);

        if (segmentType == SegmentType.InnerBull)
            return Target.Bullseye();

        if (segmentType == SegmentType.OuterBull)
            return Target.OuterBull();

        int sectorNumber = _sectorResolver.ResolveSector(point);
        return Target.Create(segmentType, sectorNumber);
    }
}
