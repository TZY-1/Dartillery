using Dartillery.Core.Enums;

namespace Dartillery.Core.Models;

/// <summary>
/// Represents the result of a dart throw.
/// Changed from sealed record to class to allow future extensibility.
/// Maintains immutability through init-only properties.
/// </summary>
public class ThrowResult
{
    /// <summary>Score achieved.</summary>
    public int Score { get; init; }

    /// <summary>Segment type hit.</summary>
    public SegmentType SegmentType { get; init; }

    /// <summary>Sector number hit (0 for bull or miss).</summary>
    public int SectorNumber { get; init; }

    /// <summary>Actual hit point on the board.</summary>
    public Point2D HitPoint { get; init; }

    /// <summary>Original aim point.</summary>
    public Point2D AimedPoint { get; init; }

    /// <summary>
    /// Optional metadata for analytics/debugging.
    /// </summary>
    public ThrowMetadata? Metadata { get; init; }

    /// <summary>
    /// Initializes a new throw result.
    /// </summary>
    /// <param name="score">The score achieved.</param>
    /// <param name="segmentType">The type of segment hit.</param>
    /// <param name="sectorNumber">The sector number (1-20), or 0 for bull/miss.</param>
    /// <param name="hitPoint">The actual hit point on the board.</param>
    /// <param name="aimedPoint">The point that was aimed at.</param>
    public ThrowResult(int score, SegmentType segmentType, int sectorNumber, Point2D hitPoint, Point2D aimedPoint)
    {
        Score = score;
        SegmentType = segmentType;
        SectorNumber = sectorNumber;
        HitPoint = hitPoint;
        AimedPoint = aimedPoint;
    }

    /// <summary>
    /// Indicates whether the throw hit the board.
    /// </summary>
    public bool IsHit => SegmentType != SegmentType.Miss;

    /// <summary>
    /// Indicates whether a bull (inner or outer) was hit.
    /// </summary>
    public bool IsBull => SegmentType is SegmentType.InnerBull or SegmentType.OuterBull;

    /// <summary>
    /// Gets a value indicating whether the segment represents a double score area or the inner bullseye.
    /// </summary>
    /// <remarks>Use this property to determine if the segment should be treated as a double for scoring
    /// purposes. The inner bullseye is typically scored as a double in standard dart games.</remarks>
    public bool IsDouble => SegmentType is SegmentType.Double or SegmentType.InnerBull;

    /// <summary>
    /// Deviation from the aim point.
    /// </summary>
    public double Deviation => HitPoint.DistanceTo(AimedPoint);

    /// <summary>
    /// Creates a miss result.
    /// </summary>
    /// <param name="hitPoint">The point where the dart landed (off board).</param>
    /// <param name="aimedPoint">The point that was aimed at.</param>
    /// <returns>A throw result representing a miss.</returns>
    public static ThrowResult Miss(Point2D hitPoint, Point2D aimedPoint) =>
        new(0, SegmentType.Miss, 0, hitPoint, aimedPoint);

    /// <inheritdoc />
    public override string ToString() => SegmentType switch
    {
        SegmentType.Miss => $"Miss at {HitPoint}",
        SegmentType.InnerBull => $"Bullseye ({Score}) at {HitPoint}",
        SegmentType.OuterBull => $"Single Bull ({Score}) at {HitPoint}",
        SegmentType.Triple => $"T{SectorNumber} ({Score}) at {HitPoint}",
        SegmentType.Double => $"D{SectorNumber} ({Score}) at {HitPoint}",
        _ => $"S{SectorNumber} ({Score}) at {HitPoint}"
    };
}
