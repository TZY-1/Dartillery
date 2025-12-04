namespace Dartillery.Core.Models;

/// <summary>
/// Represents a point in 2D coordinates on the dartboard.
/// Coordinate system: (0,0) = center, positive Y = up.
/// </summary>
public readonly struct Point2D : IEquatable<Point2D>
{
    /// <summary>Floating-point equality tolerance.</summary>
    private const double EqualityTolerance = 1e-10;

    /// <summary>X coordinate.</summary>
    public double X { get; }

    /// <summary>Y coordinate.</summary>
    public double Y { get; }

    /// <summary>
    /// Initializes a new point with the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Calculates the distance from the origin (0,0).
    /// </summary>
    public double DistanceFromOrigin => Math.Sqrt(X * X + Y * Y);

    /// <summary>
    /// Calculates the distance to another point.
    /// </summary>
    /// <param name="other">The other point.</param>
    /// <returns>Euclidean distance to the other point.</returns>
    public double DistanceTo(Point2D other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Creates a point from polar coordinates.
    /// </summary>
    /// <param name="radius">Distance from center.</param>
    /// <param name="angleFromUp">Angle in radians, 0 = up, positive = clockwise.</param>
    /// <returns>Point in Cartesian coordinates.</returns>
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

    /// <summary>Tests two points for equality.</summary>
    public static bool operator ==(Point2D a, Point2D b) => a.Equals(b);

    /// <summary>Tests two points for inequality.</summary>
    public static bool operator !=(Point2D a, Point2D b) => !a.Equals(b);

    /// <summary>
    /// Determines whether this point is equal to another within floating-point tolerance.
    /// </summary>
    public bool Equals(Point2D other) =>
        Math.Abs(X - other.X) < EqualityTolerance &&
        Math.Abs(Y - other.Y) < EqualityTolerance;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Point2D other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <inheritdoc />
    public override string ToString() => $"({X:F4}, {Y:F4})";
}
