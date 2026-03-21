namespace Dartillery.Core.Models;

/// <summary>
/// Describes the boundary shape of a spread algorithm's deviation area.
/// Each spread algorithm produces its own bounds (circle, ellipse, etc.)
/// that encapsulate truncation, visualization, and distance normalization.
/// </summary>
public interface ISpreadBounds
{
    /// <summary>
    /// Returns true if the given deviation (dx, dy) falls within this spread boundary.
    /// </summary>
    bool Contains(double dx, double dy);

    /// <summary>
    /// Returns the normalized distance from the center to the point (dx, dy)
    /// relative to this boundary. 0 = center, 1 = on the boundary, &gt;1 = outside.
    /// Enables algorithm-agnostic throw quality rating.
    /// </summary>
    double NormalizedDistance(double dx, double dy);

    /// <summary>
    /// Renders this boundary as an SVG element string centered at (cx, cy).
    /// Returns raw SVG markup (e.g. a circle or ellipse element).
    /// </summary>
    /// <param name="cx">Center X coordinate in SVG space.</param>
    /// <param name="cy">Center Y coordinate in SVG space.</param>
    /// <param name="extraAttributes">Optional additional SVG attributes (fill, stroke, etc.) injected into the element.</param>
    string ToSvgElement(double cx, double cy, string extraAttributes = "");
}
