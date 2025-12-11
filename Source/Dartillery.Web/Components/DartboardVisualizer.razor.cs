using System.Text;
using Dartillery.Shared;

namespace Dartillery.Web.Components;

public partial class DartboardVisualizer
{
    private const double InnerBullRadius = BoardDimensions.InnerBullRadius;
    private const double OuterBullRadius = BoardDimensions.OuterBullRadius;
    private const double TripleRingInner = BoardDimensions.TripleRingInner;
    private const double TripleRingOuter = BoardDimensions.TripleRingOuter;
    private const double DoubleRingInner = BoardDimensions.DoubleRingInner;
    private const double DoubleRingOuter = BoardDimensions.DoubleRingOuter;
    private const int SectorCount = BoardDimensions.SectorCount;
    private const double SectorAngle = BoardDimensions.SectorAngle;

    private static readonly int[] SectorOrder = BoardDimensions.SectorOrderClockwise;

    private const double StartAngle = -Math.PI / 2 - SectorAngle / 2;

    private string GetSectorPath(int index, double innerRadius, double outerRadius)
    {
        double startRad_clockwiseFromUp = StartAngle + index * SectorAngle;
        double angle2_clockwiseFromUp = startRad_clockwiseFromUp + SectorAngle;

        var x1 = Math.Cos(startRad_clockwiseFromUp) * innerRadius;
        var y1 = Math.Sin(startRad_clockwiseFromUp) * innerRadius;
        var x2 = Math.Cos(angle2_clockwiseFromUp) * innerRadius;
        var y2 = Math.Sin(angle2_clockwiseFromUp) * innerRadius;
        var x3 = Math.Cos(angle2_clockwiseFromUp) * outerRadius;
        var y3 = Math.Sin(angle2_clockwiseFromUp) * outerRadius;
        var x4 = Math.Cos(startRad_clockwiseFromUp) * outerRadius;
        var y4 = Math.Sin(startRad_clockwiseFromUp) * outerRadius;

        int largeArcFlag = (SectorAngle > Math.PI) ? 1 : 0;

        var sb = new StringBuilder();
        var culture = System.Globalization.CultureInfo.InvariantCulture;

        sb.Append($"M {x1.ToString("F4", culture)} {y1.ToString("F4", culture)} ");
        sb.Append($"A {innerRadius.ToString("F4", culture)} {innerRadius.ToString("F4", culture)} 0 {largeArcFlag} 1 {x2.ToString("F4", culture)} {y2.ToString("F4", culture)} ");
        sb.Append($"L {x3.ToString("F4", culture)} {y3.ToString("F4", culture)} ");
        sb.Append($"A {outerRadius.ToString("F4", culture)} {outerRadius.ToString("F4", culture)} 0 {largeArcFlag} 0 {x4.ToString("F4", culture)} {y4.ToString("F4", culture)} Z");

        return sb.ToString();
    }

    private (double X, double Y) GetSectorTextPosition(int index)
    {
        var angle = StartAngle + index * SectorAngle + SectorAngle / 2;
        var radius = DoubleRingOuter + 0.12;

        return (
            Math.Cos(angle) * radius,
            Math.Sin(angle) * radius
        );
    }
}
