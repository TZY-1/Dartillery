using Dartillery.Core.Constants;
using Dartillery.Core.Enums;

namespace Dartillery.Simulation.Geometry;

/// <summary>
/// Default implementation of dartboard score calculation.
/// </summary>
internal sealed class ScoreCalculator : IScoreCalculator
{
    public int CalculateScore(SegmentType segmentType, int sectorNumber)
    {
        return segmentType switch
        {
            SegmentType.InnerBull => BoardScoring.InnerBullScore,
            SegmentType.OuterBull => BoardScoring.OuterBullScore,
            SegmentType.Triple => sectorNumber * BoardScoring.TripleMultiplier,
            SegmentType.Double => sectorNumber * BoardScoring.DoubleMultiplier,
            SegmentType.Single => sectorNumber * BoardScoring.SingleMultiplier,
            SegmentType.Miss => 0,
            _ => throw new ArgumentOutOfRangeException(
                nameof(segmentType),
                $"Unknown segment type: {segmentType}")
        };
    }
}
