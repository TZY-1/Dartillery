using Dartillery.Core.Enums;

namespace Dartillery.Core.Models;

/// <summary>
/// Immutable result of a single dart throw, including the segment hit, score, and actual/aimed coordinates.
/// Use <see cref="Miss(Point2D, Point2D)"/> to create off-board results.
/// </summary>
public sealed record ThrowResult
{
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

    /// <summary>Points scored by this throw (0 for a miss).</summary>
    public int Score { get; init; }

    /// <summary>The type of board segment hit (single, double, triple, bull, or miss).</summary>
    public SegmentType SegmentType { get; init; }

    /// <summary>Sector number hit (1-20), or 0 for bull or miss.</summary>
    public int SectorNumber { get; init; }

    /// <summary>Actual landing point on the board in normalized coordinates.</summary>
    public Point2D HitPoint { get; init; }

    /// <summary>Point the player was aiming at in normalized coordinates.</summary>
    public Point2D AimedPoint { get; init; }

    /// <summary>
    /// Optional per-throw analytics data (modifiers applied, player name, session ID, timestamp).
    /// </summary>
    public ThrowMetadata? Metadata { get; init; }

    /// <summary>True when the dart landed on any scoring segment (not a miss).</summary>
    public bool IsHit => SegmentType != SegmentType.Miss;

    /// <summary>True when the dart hit the inner or outer bull.</summary>
    public bool IsBull => SegmentType is SegmentType.InnerBull or SegmentType.OuterBull;

    /// <summary>
    /// True when the segment counts as a double for checkout purposes (double ring or inner bull).
    /// </summary>
    public bool IsDouble => SegmentType is SegmentType.Double or SegmentType.InnerBull;

    /// <summary>Euclidean distance between <see cref="HitPoint"/> and <see cref="AimedPoint"/> in normalized units.</summary>
    public double Deviation => HitPoint.DistanceTo(AimedPoint);

    /// <summary>
    /// Creates a throw result representing a dart that landed off the board.
    /// </summary>
    /// <param name="hitPoint">The point where the dart landed (outside the scoring area).</param>
    /// <param name="aimedPoint">The point that was aimed at.</param>
    /// <returns>A <see cref="ThrowResult"/> with zero score and <see cref="SegmentType.Miss"/>.</returns>
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
