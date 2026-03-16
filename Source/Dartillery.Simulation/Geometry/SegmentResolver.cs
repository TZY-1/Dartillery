using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;
using Dartillery.Shared;

namespace Dartillery.Simulation.Geometry;

/// <summary>
/// Coordinates sector, ring, and score resolution to produce complete throw results.
/// </summary>
internal sealed class SegmentResolver : ISegmentResolver
{
    private readonly ISectorResolver _sectorResolver;
    private readonly IRingResolver _ringResolver;
    private readonly IScoreCalculator _scoreCalculator;

    /// <summary>
    /// Creates a segment resolver with default implementations.
    /// </summary>
    public SegmentResolver()
        : this(new SectorResolver(), new RingResolver(), new ScoreCalculator())
    {
    }

    /// <summary>
    /// Creates a segment resolver with custom implementations (for testing/extensibility).
    /// </summary>
    internal SegmentResolver(
        ISectorResolver sectorResolver,
        IRingResolver ringResolver,
        IScoreCalculator scoreCalculator)
    {
        ArgumentNullException.ThrowIfNull(sectorResolver);
        ArgumentNullException.ThrowIfNull(ringResolver);
        ArgumentNullException.ThrowIfNull(scoreCalculator);
        _sectorResolver = sectorResolver;
        _ringResolver = ringResolver;
        _scoreCalculator = scoreCalculator;
    }

    public ThrowResult Resolve(Point2D hitPoint, Point2D aimedPoint)
    {
        double radius = hitPoint.DistanceFromOrigin;

        if (radius > BoardDimensions.BoardRadius)
            return ThrowResult.Miss(hitPoint, aimedPoint);

        SegmentType segmentType = _ringResolver.ResolveRing(radius);

        if (segmentType is SegmentType.InnerBull or SegmentType.OuterBull)
        {
            int bullScore = _scoreCalculator.CalculateScore(segmentType, 0);
            return new ThrowResult(bullScore, segmentType, 0, hitPoint, aimedPoint);
        }

        int sectorNumber = _sectorResolver.ResolveSector(hitPoint);
        int score = _scoreCalculator.CalculateScore(segmentType, sectorNumber);
        return new ThrowResult(score, segmentType, sectorNumber, hitPoint, aimedPoint);
    }
}
