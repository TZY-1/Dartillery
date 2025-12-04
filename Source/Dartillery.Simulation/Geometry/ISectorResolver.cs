using Dartillery.Core.Models;

namespace Dartillery.Simulation.Geometry;

/// <summary>
/// Resolves dartboard coordinates to sector numbers (1-20).
/// </summary>
internal interface ISectorResolver
{
    /// <summary>
    /// Determines which numbered sector (1-20) the point falls in.
    /// </summary>
    /// <param name="point">Point on the dartboard in Cartesian coordinates.</param>
    /// <returns>Sector number from 1 to 20.</returns>
    int ResolveSector(Point2D point);
}
