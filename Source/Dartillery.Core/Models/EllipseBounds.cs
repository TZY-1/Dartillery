using System.Globalization;

namespace Dartillery.Core.Models;

/// <summary>
/// Elliptical spread boundary defined by two semi-axes and a rotation angle.
/// Models directional accuracy differences in realistic dart throwing.
/// </summary>
public sealed record EllipseBounds(double RadiusX, double RadiusY, double AngleDegrees = 0) : ISpreadBounds
{
    private const double _degToRad = Math.PI / 180.0;

    /// <inheritdoc/>
    public bool Contains(double dx, double dy)
    {
        var (lx, ly) = RotateToLocal(dx, dy);
        return RadiusX > 0 && RadiusY > 0
            && ((lx * lx) / (RadiusX * RadiusX)) + ((ly * ly) / (RadiusY * RadiusY)) <= 1.0;
    }

    /// <inheritdoc/>
    public double NormalizedDistance(double dx, double dy)
    {
        if (RadiusX <= 0 || RadiusY <= 0) return 0;
        var (lx, ly) = RotateToLocal(dx, dy);
        return Math.Sqrt(((lx * lx) / (RadiusX * RadiusX)) + ((ly * ly) / (RadiusY * RadiusY)));
    }

    /// <inheritdoc/>
    public string ToSvgElement(double cx, double cy, string extraAttributes = "")
    {
        var c = CultureInfo.InvariantCulture;
        var extra = string.IsNullOrEmpty(extraAttributes) ? string.Empty : " " + extraAttributes;

        // SVG rotate uses clockwise degrees; negate because our Y-axis is inverted in SVG
        var svgAngle = -AngleDegrees;

        return $"<ellipse cx=\"{cx.ToString("F4", c)}\" cy=\"{cy.ToString("F4", c)}\" " +
               $"rx=\"{RadiusX.ToString("F4", c)}\" ry=\"{RadiusY.ToString("F4", c)}\" " +
               $"transform=\"rotate({svgAngle.ToString("F1", c)} {cx.ToString("F4", c)} {cy.ToString("F4", c)})\"{extra} />";
    }

    /// <summary>
    /// Rotates a point by the negative of the ellipse angle to align with the local (unrotated) axes.
    /// </summary>
    private (double X, double Y) RotateToLocal(double dx, double dy)
    {
        if (AngleDegrees == 0) return (dx, dy);
        double rad = -AngleDegrees * _degToRad;
        double cos = Math.Cos(rad);
        double sin = Math.Sin(rad);
        return ((dx * cos) - (dy * sin), (dx * sin) + (dy * cos));
    }
}
