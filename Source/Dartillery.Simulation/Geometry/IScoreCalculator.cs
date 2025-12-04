using Dartillery.Core.Enums;

namespace Dartillery.Simulation.Geometry;

/// <summary>
/// Calculates dart scores based on segment type and sector number.
/// </summary>
internal interface IScoreCalculator
{
    /// <summary>
    /// Calculates the point value for a dart landing in the specified segment.
    /// </summary>
    /// <param name="segmentType">The type of segment hit.</param>
    /// <param name="sectorNumber">The sector number (1-20), or 0 for bull/miss.</param>
    /// <returns>The score in points.</returns>
    int CalculateScore(SegmentType segmentType, int sectorNumber);
}
