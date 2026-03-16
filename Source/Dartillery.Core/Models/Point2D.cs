namespace Dartillery.Core.Models;

/// <summary>
/// Immutable 2D coordinate on the dartboard. Origin (0,0) is the board center; positive Y is up.
/// Equality is tolerance-based (<c>1e-10</c>) to avoid floating-point precision issues.
/// </summary>
public readonly record struct Point2D(double X, double Y)
{
    /// <summary>Floating-point equality tolerance.</summary>
    private const double EqualityTolerance = 1e-10;

    /// <summary>
    /// Euclidean distance from the board center (0,0), i.e., the radial position.
    /// </summary>
    public double DistanceFromOrigin => Math.Sqrt(X * X + Y * Y);

    /// <summary>
    /// Returns the Euclidean distance to another point.
    /// </summary>
    /// <param name="other">The point to measure to.</param>
    /// <returns>Euclidean distance in the same units as the coordinates.</returns>
    public double DistanceTo(Point2D other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Creates a point from polar coordinates using dartboard angular convention (0 rad = 12 o'clock, positive = clockwise).
    /// </summary>
    /// <param name="radius">Radial distance from the center.</param>
    /// <param name="angleFromUp">Angle in radians; 0 = up (12 o'clock), positive values rotate clockwise.</param>
    /// <returns>Equivalent Cartesian <see cref="Point2D"/>.</returns>
    public static Point2D FromPolar(double radius, double angleFromUp)
    {
        double standardAngle = Math.PI / 2.0 - angleFromUp;
        double x = radius * Math.Cos(standardAngle);
        double y = radius * Math.Sin(standardAngle);
        return new Point2D(x, y);
    }

    /// <summary>
    /// Origin point (0,0).
    /// </summary>
    public static Point2D Origin => new(0, 0);

    /// <summary>Adds two points component-wise.</summary>
    public static Point2D operator +(Point2D a, Point2D b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>Subtracts two points component-wise.</summary>
    public static Point2D operator -(Point2D a, Point2D b) => new(a.X - b.X, a.Y - b.Y);

    /// <summary>
    /// Determines whether this point is equal to another within floating-point tolerance.
    /// </summary>
    public bool Equals(Point2D other) =>
        Math.Abs(X - other.X) < EqualityTolerance &&
        Math.Abs(Y - other.Y) < EqualityTolerance;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <inheritdoc />
    public override string ToString() => $"({X:F4}, {Y:F4})";
}
