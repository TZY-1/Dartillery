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
        _sectorResolver = sectorResolver ?? throw new ArgumentNullException(nameof(sectorResolver));
        _ringResolver = ringResolver ?? throw new ArgumentNullException(nameof(ringResolver));
        _scoreCalculator = scoreCalculator ?? throw new ArgumentNullException(nameof(scoreCalculator));
    }

    public ThrowResult Resolve(Point2D hitPoint, Point2D aimedPoint)
    {
        double radius = hitPoint.DistanceFromOrigin;

        // Miss - outside the board boundary
        if (radius > BoardDimensions.BoardRadius)
        {
            return ThrowResult.Miss(hitPoint, aimedPoint);
        }

        // Determine ring type
        SegmentType segmentType = _ringResolver.ResolveRing(radius);

        // Bull segments don't have sector numbers
        if (segmentType is SegmentType.InnerBull or SegmentType.OuterBull)
        {
            int bullScore = _scoreCalculator.CalculateScore(segmentType, 0);
            return new ThrowResult(bullScore, segmentType, 0, hitPoint, aimedPoint);
        }

        // Resolve sector and calculate score for numbered segments
        int sectorNumber = _sectorResolver.ResolveSector(hitPoint);
        int score = _scoreCalculator.CalculateScore(segmentType, sectorNumber);

        return new ThrowResult(score, segmentType, sectorNumber, hitPoint, aimedPoint);
    }
}
