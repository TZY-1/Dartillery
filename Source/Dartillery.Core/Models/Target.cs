using Dartillery.Core.Constants;
using Dartillery.Core.Enums;

namespace Dartillery.Core.Models;

/// <summary>
/// Represents a target on the dartboard.
/// </summary>
public sealed class Target : IEquatable<Target>
{
    private Target(SegmentType segmentType, int sectorNumber)
    {
        SegmentType = segmentType;
        SectorNumber = sectorNumber;
    }

    /// <summary>The segment type (Single, Double, Triple, Bull).</summary>
    public SegmentType SegmentType { get; }

    /// <summary>The sector number (1-20). Irrelevant for bull targets.</summary>
    public int SectorNumber { get; }

    /// <summary>
    /// Creates a target for a numbered field (Single, Double, Triple).
    /// </summary>
    /// <param name="segmentType">The type of segment.</param>
    /// <param name="sectorNumber">The sector number (1-20).</param>
    /// <returns>A new target.</returns>
    public static Target Create(SegmentType segmentType, int sectorNumber)
    {
        if (segmentType is SegmentType.InnerBull or SegmentType.OuterBull)
        {
            return new Target(segmentType, 0);
        }

        if (sectorNumber < 1 || sectorNumber > 20)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sectorNumber),
                "Sector number must be between 1 and 20.");
        }

        if (segmentType == SegmentType.Miss)
        {
            throw new ArgumentException("Miss is not a valid target.", nameof(segmentType));
        }

        return new Target(segmentType, sectorNumber);
    }

    /// <summary>
    /// Creates a triple target.
    /// </summary>
    /// <param name="sectorNumber">The sector number (1-20).</param>
    /// <returns>A triple target.</returns>
    public static Target Triple(int sectorNumber) => Create(SegmentType.Triple, sectorNumber);

    /// <summary>
    /// Creates a double target.
    /// </summary>
    /// <param name="sectorNumber">The sector number (1-20).</param>
    /// <returns>A double target.</returns>
    public static Target Double(int sectorNumber) => Create(SegmentType.Double, sectorNumber);

    /// <summary>
    /// Creates a single target.
    /// </summary>
    /// <param name="sectorNumber">The sector number (1-20).</param>
    /// <returns>A single target.</returns>
    public static Target Single(int sectorNumber) => Create(SegmentType.Single, sectorNumber);

    /// <summary>
    /// Creates a bullseye (inner bull) target worth 50 points.
    /// </summary>
    /// <returns>A bullseye target.</returns>
    public static Target Bullseye() => Create(SegmentType.InnerBull, 0);

    /// <summary>
    /// Creates an outer bull (single bull) target worth 25 points.
    /// </summary>
    /// <returns>An outer bull target.</returns>
    public static Target OuterBull() => Create(SegmentType.OuterBull, 0);

    /// <summary>
    /// Calculates the point value of this target.
    /// </summary>
    /// <returns>The score value in points.</returns>
    public int GetScore() => SegmentType switch
    {
        SegmentType.InnerBull => BoardScoring.InnerBullScore,
        SegmentType.OuterBull => BoardScoring.OuterBullScore,
        SegmentType.Triple => SectorNumber * BoardScoring.TripleMultiplier,
        SegmentType.Double => SectorNumber * BoardScoring.DoubleMultiplier,
        SegmentType.Single => SectorNumber * BoardScoring.SingleMultiplier,
        _ => 0
    };

    /// <inheritdoc />
    public bool Equals(Target? other)
    {
        if (other is null) return false;
        return SegmentType == other.SegmentType && SectorNumber == other.SectorNumber;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Target);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(SegmentType, SectorNumber);

    /// <inheritdoc />
    public override string ToString() => SegmentType switch
    {
        SegmentType.InnerBull => "Bullseye",
        SegmentType.OuterBull => "Single Bull",
        SegmentType.Triple => $"T{SectorNumber}",
        SegmentType.Double => $"D{SectorNumber}",
        SegmentType.Single => $"S{SectorNumber}",
        _ => "Miss"
    };
}
