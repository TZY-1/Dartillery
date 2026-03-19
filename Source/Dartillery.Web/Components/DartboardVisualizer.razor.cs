using System.Text;
using Dartillery.Core.Models;
using Dartillery.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

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

    private const double StartAngle = (-Math.PI / 2) - (SectorAngle / 2);

    private static readonly int[] SectorOrder = BoardDimensions.SectorOrderClockwise;

    private ElementReference svgElementRef;

#pragma warning disable CA2227 // Blazor [Parameter] properties require a setter
    [Parameter]
    public List<ThrowResult>? Throws { get; set; }
#pragma warning restore CA2227

    [Parameter]
    public bool ShowDeviationLines { get; set; } = true;

    [Parameter]
    public bool ShowSpreadCircle { get; set; }

    [Parameter]
    public double SpreadRadius { get; set; }

    [Parameter]
    public bool EnableManualTargeting { get; set; }

    [Parameter]
    public EventCallback<(double X, double Y)> OnManualTargetSelected { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private static string GetSectorPath(int index, double innerRadius, double outerRadius)
    {
        double startRad_clockwiseFromUp = StartAngle + (index * SectorAngle);
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

    private static (double X, double Y) GetSectorTextPosition(int index)
    {
        var angle = StartAngle + (index * SectorAngle) + (SectorAngle / 2);
        var radius = DoubleRingOuter + 0.12;

        return (Math.Cos(angle) * radius, Math.Sin(angle) * radius);
    }

    private async Task HandleSvgClick(MouseEventArgs e)
    {
        if (!EnableManualTargeting) return;

        try
        {
            // Play throw sound
            await JSRuntime.InvokeVoidAsync("playAudio", "/sounds/throw.mp3");

            // Get SVG coordinates from screen coordinates
            var coords = await JSRuntime.InvokeAsync<SvgCoordinates>(
                "dartboardInterop.getSvgCoordinates",
                svgElementRef,
                e.ClientX,
                e.ClientY);

            // Invoke callback with normalized coordinates
            await OnManualTargetSelected.InvokeAsync((coords.X, coords.Y));

            // Play impact sound after a short delay (simulating dart flight)
            await Task.Delay(300);
            await JSRuntime.InvokeVoidAsync("playAudio", "/sounds/impact.mp3");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling SVG click: {ex.Message}");
        }
    }

#pragma warning disable CA1812 // Instantiated by JSInterop deserialization
#pragma warning disable S1144 // Properties set by JSInterop deserialization via reflection
    private sealed class SvgCoordinates
    {
        public double X { get; set; }

        public double Y { get; set; }
    }
#pragma warning restore S1144
#pragma warning restore CA1812
}
