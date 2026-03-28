using Dartillery.Core.Models;
using Dartillery.Shared;

namespace Dartillery.Simulation.Geometry;

/// <summary>
/// Default implementation of sector resolution using standard dartboard layout.
/// </summary>
internal sealed class SectorResolver : ISectorResolver
{
    /// <inheritdoc/>
    public int ResolveSector(Point2D point)
    {
        // Calculate angle from positive X-axis
        double angleFromX = Math.Atan2(point.Y, point.X);

        // Convert to angle from top (12 o'clock), clockwise
        double angleFromUp = (Math.PI / 2.0) - angleFromX;

        // Normalize to [0, 2π) using modulo (replaces while loops - fixes infinite loop bug)
        angleFromUp = ((angleFromUp % (2.0 * Math.PI)) + (2.0 * Math.PI)) % (2.0 * Math.PI);

        // Calculate sector index - offset by half sector to center sectors on their nominal angles
        // Each sector should be centered on its angle, not starting at its angle
        // For example, Sector 20 (angle 0°) should cover -9° to +9°, not 0° to +18°
        const double halfSector = BoardDimensions.SectorAngle / 2.0;
        double offsetAngle = angleFromUp + halfSector;
        int sectorIndex = (int)(offsetAngle / BoardDimensions.SectorAngle) % BoardDimensions.SectorCount;

        return BoardDimensions.SectorOrderClockwise[sectorIndex];
    }
}
