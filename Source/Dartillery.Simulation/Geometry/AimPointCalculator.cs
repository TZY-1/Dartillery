using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;
using Dartillery.Shared;

namespace Dartillery.Simulation.Geometry;

/// <summary>
/// Calculates optimal aim points for dartboard targets.
/// </summary>
internal sealed class AimPointCalculator : IAimPointCalculator
{
    /// <summary>
    /// Predefined aim radii for different segment types.
    /// </summary>
    private static class AimRadii
    {
        /// <summary>Aim for center of board (inner bull).</summary>
        public const double InnerBull = 0.0;

        /// <summary>Midpoint of outer bull ring.</summary>
        public static readonly double OuterBull =
            (BoardDimensions.InnerBullRadius + BoardDimensions.OuterBullRadius) / 2.0;

        /// <summary>Midpoint of triple ring.</summary>
        public static readonly double Triple =
            (BoardDimensions.TripleRingInner + BoardDimensions.TripleRingOuter) / 2.0;

        /// <summary>Midpoint of double ring.</summary>
        public static readonly double Double =
            (BoardDimensions.DoubleRingInner + BoardDimensions.DoubleRingOuter) / 2.0;

        /// <summary>Midpoint of outer single area (between triple and double rings).</summary>
        public static readonly double Single =
            (BoardDimensions.TripleRingOuter + BoardDimensions.DoubleRingInner) / 2.0;
    }

    /// <inheritdoc />
    public Point2D CalculateAimPoint(Target target)
    {
        double radius = GetAimRadius(target.SegmentType);

        // For bulls, the angle is irrelevant (centered)
        if (target.SegmentType is SegmentType.InnerBull or SegmentType.OuterBull)
        {
            return Point2D.FromPolar(radius, 0);
        }

        double angle = GetSectorCenterAngle(target.SectorNumber);
        return Point2D.FromPolar(radius, angle);
    }

    private static double GetAimRadius(SegmentType segmentType) => segmentType switch
    {
        SegmentType.InnerBull => AimRadii.InnerBull,
        SegmentType.OuterBull => AimRadii.OuterBull,
        SegmentType.Triple => AimRadii.Triple,
        SegmentType.Double => AimRadii.Double,
        SegmentType.Single => AimRadii.Single,
        _ => AimRadii.Single
    };

    private static double GetSectorCenterAngle(int sectorNumber)
    {
        int index = Array.IndexOf(BoardDimensions.SectorOrderClockwise, sectorNumber);

        if (index < 0)
        {
            throw new ArgumentException($"Invalid sector number: {sectorNumber}", nameof(sectorNumber));
        }

        // Angle to the center of the sector
        return index * BoardDimensions.SectorAngle;
    }
}
