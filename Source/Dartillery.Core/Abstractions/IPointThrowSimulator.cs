using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Interface for simulating dart throws at specific board coordinates.
/// </summary>
internal interface IPointThrowSimulator
{
    /// <summary>
    /// Simulates a throw at the specified point on the board.
    /// </summary>
    /// <param name="aimPoint">The point to aim for in board coordinates.</param>
    /// <returns>The throw result including actual hit location and score.</returns>
    ThrowResult ThrowAt(Point2D aimPoint);
}
