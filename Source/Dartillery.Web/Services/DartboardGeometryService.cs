using Dartillery.Core.Models;
using Dartillery.Shared;

namespace Dartillery.Web.Services;

/// <summary>
/// Service for transforming dartboard coordinates between normalized and viewport coordinates.
/// Singleton service as it is stateless and only performs calculations.
/// SOLID: Single Responsibility Principle - coordinate transformation only
/// </summary>
public static class DartboardGeometryService
{
    private const double _viewBoxCenter = 300.0;

    /// <summary>
    /// Converts normalized coordinates (0-1 radius) to SVG viewport pixels
    /// </summary>
    /// <param name="point">Normalized point where (0,0) = center</param>
    /// <param name="viewportRadius">Viewport radius in pixels (default: 280px)</param>
    /// <returns>Tuple with (X, Y) viewport coordinates</returns>
    public static (double X, double Y) NormalizedToViewport(Point2D point, double viewportRadius = 280.0)
    {
        double x = _viewBoxCenter + (point.X * viewportRadius);
        double y = _viewBoxCenter - (point.Y * viewportRadius); // Invert Y-axis

        return (x, y);
    }

    /// <summary>
    /// Calculates the center angle of a sector in radians
    /// </summary>
    /// <param name="sectorIndex">Index in SectorOrderClockwise array (0-19)</param>
    /// <returns>Angle in radians from 12 o'clock clockwise</returns>
    public static double GetSectorCenterAngle(int sectorIndex)
    {
        const double sectorAngle = 2.0 * Math.PI / 20.0; // 18° in radians
        return sectorIndex * sectorAngle;
    }

    /// <summary>
    /// Returns start and end angles for a sector
    /// </summary>
    /// <param name="sectorIndex">Index in SectorOrderClockwise array (0-19)</param>
    /// <returns>Tuple with (StartAngle, EndAngle) in radians</returns>
    public static (double StartAngle, double EndAngle) GetSectorBoundaryAngles(int sectorIndex)
    {
        const double sectorAngle = 2.0 * Math.PI / 20.0;
        const double halfSectorAngle = sectorAngle / 2.0;

        double centerAngle = GetSectorCenterAngle(sectorIndex);
        double startAngle = centerAngle - halfSectorAngle;
        double endAngle = centerAngle + halfSectorAngle;

        return (startAngle, endAngle);
    }

    /// <summary>
    /// Checks if a sector number is red (otherwise green/black)
    /// </summary>
    /// <param name="sectorNumber">Sector number 1-20</param>
    /// <returns>True if red, false if green</returns>
    public static bool IsRedSector(int sectorNumber)
    {
        // Red sectors: 1, 18, 4, 13, 6, 10, 15, 2, 17, 3, 19, 7, 16, 8, 11, 14, 9, 12, 5
        // Pattern: odd positions in clockwise array are red
        int[] redSectors = { 1, 18, 4, 13, 6, 10, 15, 2, 17, 3, 19, 7, 16, 8, 11, 14, 9, 12, 5 };
        return Array.IndexOf(redSectors, sectorNumber) >= 0;
    }

    /// <summary>
    /// Returns the sector order clockwise from 12 o'clock
    /// </summary>
    public static int[] GetSectorOrderClockwise() => BoardDimensions.SectorOrderClockwise;

    /// <summary>
    /// Converts polar coordinates (radius, angle) to normalized Cartesian coordinates
    /// </summary>
    /// <param name="radius">Radius (0-1, normalized)</param>
    /// <param name="angleFromTop">Angle from 12 o'clock clockwise in radians</param>
    /// <returns>Point2D with normalized X,Y coordinates</returns>
    public static Point2D PolarToNormalized(double radius, double angleFromTop)
    {
        return Point2D.FromPolar(radius, angleFromTop);
    }

    /// <summary>
    /// Calculates SVG path string for a circular arc
    /// </summary>
    /// <param name="centerX">Center X coordinate</param>
    /// <param name="centerY">Center Y coordinate</param>
    /// <param name="radius">Arc radius</param>
    /// <param name="startAngle">Start angle in radians (from 12 o'clock clockwise)</param>
    /// <param name="endAngle">End angle in radians (from 12 o'clock clockwise)</param>
    /// <returns>SVG path string for the arc</returns>
    public static string CreateArcPath(double centerX, double centerY, double radius, double startAngle, double endAngle)
    {
        // Convert from "angle from top clockwise" to standard coordinate system
        double startX = centerX + (radius * Math.Sin(startAngle));
        double startY = centerY - (radius * Math.Cos(startAngle));
        double endX = centerX + (radius * Math.Sin(endAngle));
        double endY = centerY - (radius * Math.Cos(endAngle));

        // Large arc flag: 1 if arc > 180°, otherwise 0
        int largeArcFlag = (endAngle - startAngle) > Math.PI ? 1 : 0;

        // SVG Arc Path: M startX,startY A radius,radius 0 largeArcFlag sweepFlag endX,endY
        // sweepFlag = 1 for clockwise
        return $"M {startX:F2},{startY:F2} A {radius:F2},{radius:F2} 0 {largeArcFlag} 1 {endX:F2},{endY:F2}";
    }

    /// <summary>
    /// Creates SVG path for a sector segment (e.g., triple ring segment)
    /// </summary>
    /// <param name="centerX">Center X coordinate</param>
    /// <param name="centerY">Center Y coordinate</param>
    /// <param name="innerRadius">Inner radius</param>
    /// <param name="outerRadius">Outer radius</param>
    /// <param name="startAngle">Start angle in radians</param>
    /// <param name="endAngle">End angle in radians</param>
    /// <returns>Closed SVG path for the segment</returns>
    public static string CreateSectorSegmentPath(double centerX, double centerY, double innerRadius, double outerRadius, double startAngle, double endAngle)
    {
        // Outer arc points
        double outerStartX = centerX + (outerRadius * Math.Sin(startAngle));
        double outerStartY = centerY - (outerRadius * Math.Cos(startAngle));
        double outerEndX = centerX + (outerRadius * Math.Sin(endAngle));
        double outerEndY = centerY - (outerRadius * Math.Cos(endAngle));

        // Inner arc points
        double innerStartX = centerX + (innerRadius * Math.Sin(startAngle));
        double innerStartY = centerY - (innerRadius * Math.Cos(startAngle));
        double innerEndX = centerX + (innerRadius * Math.Sin(endAngle));
        double innerEndY = centerY - (innerRadius * Math.Cos(endAngle));

        int largeArcFlag = (endAngle - startAngle) > Math.PI ? 1 : 0;

        // Path: Start at outer arc, arc outward, line inward, arc back, line to close
        return $"M {outerStartX:F2},{outerStartY:F2} " +
               $"A {outerRadius:F2},{outerRadius:F2} 0 {largeArcFlag} 1 {outerEndX:F2},{outerEndY:F2} " +
               $"L {innerEndX:F2},{innerEndY:F2} " +
               $"A {innerRadius:F2},{innerRadius:F2} 0 {largeArcFlag} 0 {innerStartX:F2},{innerStartY:F2} " +
               "Z";
    }
}
