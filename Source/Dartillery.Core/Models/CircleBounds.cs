using System.Globalization;

namespace Dartillery.Core.Models;

/// <summary>
/// Circular spread boundary defined by a single radius.
/// </summary>
public sealed record CircleBounds(double Radius) : ISpreadBounds
{
    public bool Contains(double dx, double dy)
        => (dx * dx) + (dy * dy) <= Radius * Radius;

    public double NormalizedDistance(double dx, double dy)
        => Radius > 0 ? Math.Sqrt((dx * dx) + (dy * dy)) / Radius : 0;

    public string ToSvgElement(double cx, double cy, string extraAttributes = "")
    {
        var c = CultureInfo.InvariantCulture;
        var extra = string.IsNullOrEmpty(extraAttributes) ? string.Empty : " " + extraAttributes;
        return $"<circle cx=\"{cx.ToString("F4", c)}\" cy=\"{cy.ToString("F4", c)}\" r=\"{Radius.ToString("F4", c)}\"{extra} />";
    }
}
