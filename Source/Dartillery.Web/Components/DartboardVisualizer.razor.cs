using System.Text;
using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;
using Dartillery.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Dartillery.Web.Components;

public partial class DartboardVisualizer
{
    private const double _innerBullRadius = BoardDimensions.InnerBullRadius;

    private const double _outerBullRadius = BoardDimensions.OuterBullRadius;

    private const double _tripleRingInner = BoardDimensions.TripleRingInner;

    private const double _tripleRingOuter = BoardDimensions.TripleRingOuter;

    private const double _doubleRingInner = BoardDimensions.DoubleRingInner;

    private const double _doubleRingOuter = BoardDimensions.DoubleRingOuter;

    private const int _sectorCount = BoardDimensions.SectorCount;

    private const double _sectorAngle = BoardDimensions.SectorAngle;

    private const double _startAngle = (-Math.PI / 2) - (_sectorAngle / 2);

    private static readonly int[] _sectorOrder = BoardDimensions.SectorOrderClockwise;

    private ElementReference _svgElementRef;
    private bool _mouseOver;
    private double _mouseX;
    private double _mouseY;
    private int _hoveredThrowIndex = -1;

/// <summary>Throw results to render on the board.</summary>
#pragma warning disable CA2227 // Blazor [Parameter] properties require a setter
    [Parameter]
    public List<ThrowResult>? Throws { get; set; }
#pragma warning restore CA2227

    /// <summary>Whether to draw deviation lines from aim to hit point.</summary>
    [Parameter]
    public bool ShowDeviationLines { get; set; } = true;

    /// <summary>Whether to show the spread shape overlay.</summary>
    [Parameter]
    public bool ShowSpreadCircle { get; set; }

    /// <summary>Base spread bounds (without fatigue).</summary>
    [Parameter]
    public ISpreadBounds? BaseBounds { get; set; }

    /// <summary>Effective spread bounds (with fatigue).</summary>
    [Parameter]
    public ISpreadBounds? EffectiveBounds { get; set; }

    /// <summary>Whether clicking the board triggers a manual throw.</summary>
    [Parameter]
    public bool EnableManualTargeting { get; set; }

    /// <summary>Whether dart grouping/deflection is active.</summary>
    [Parameter]
    public bool EnableGrouping { get; set; }

    /// <summary>Cluster radius for grouping visualization.</summary>
    [Parameter]
    public double GroupingClusterRadius { get; set; }

    /// <summary>Whether the zoom inset is enabled.</summary>
    [Parameter]
    public bool EnableZoom { get; set; }

    /// <summary>Zoom magnification level.</summary>
    [Parameter]
    public double ZoomLevel { get; set; } = 0.08;

    /// <summary>Zoom inset viewport size.</summary>
    [Parameter]
    public double ZoomSize { get; set; } = 0.64;

    /// <summary>Callback when user clicks the board for manual targeting.</summary>
    [Parameter]
    public EventCallback<(double X, double Y)> OnManualTargetSelected { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private static string GetSectorPath(int index, double innerRadius, double outerRadius)
    {
        double startRad_clockwiseFromUp = _startAngle + (index * _sectorAngle);
        double angle2_clockwiseFromUp = startRad_clockwiseFromUp + _sectorAngle;

        var x1 = Math.Cos(startRad_clockwiseFromUp) * innerRadius;
        var y1 = Math.Sin(startRad_clockwiseFromUp) * innerRadius;
        var x2 = Math.Cos(angle2_clockwiseFromUp) * innerRadius;
        var y2 = Math.Sin(angle2_clockwiseFromUp) * innerRadius;
        var x3 = Math.Cos(angle2_clockwiseFromUp) * outerRadius;
        var y3 = Math.Sin(angle2_clockwiseFromUp) * outerRadius;
        var x4 = Math.Cos(startRad_clockwiseFromUp) * outerRadius;
        var y4 = Math.Sin(startRad_clockwiseFromUp) * outerRadius;

        const int largeArcFlag = (_sectorAngle > Math.PI) ? 1 : 0;

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
        var angle = _startAngle + (index * _sectorAngle) + (_sectorAngle / 2);
        const double radius = _doubleRingOuter + 0.12;

        return (Math.Cos(angle) * radius, Math.Sin(angle) * radius);
    }

    private (double X, double Y) GetZoomPosition()
    {
        const double inset = 0.02;
        const double border = inset + 0.04;
        var totalSize = ZoomSize + border;

        double x = _mouseX >= 0 ? -1.15 + inset : 1.15 - totalSize;
        double y = _mouseY >= 0 ? -1.15 + inset : 1.15 - totalSize;
        return (x, y);
    }

    private Point2D GetZoomCenter()
    {
        return new Point2D(_mouseX, _mouseY);
    }

    private async Task HandleMouseMove(MouseEventArgs e)
    {
        try
        {
            var coords = await JSRuntime.InvokeAsync<SvgCoordinates>(
                "dartboardInterop.getSvgCoordinates",
                _svgElementRef,
                e.ClientX,
                e.ClientY);

            _mouseX = coords.X;
            _mouseY = coords.Y;
            _mouseOver = true;
        }
        catch
        {
            // JS interop can fail during rapid mouse movement if the SVG element
            // is not yet rendered or has been disposed. Safe to ignore — coordinates
            // will update on the next mouse event.
        }
    }

    private void HandleMouseLeave(MouseEventArgs mouseEventArgs)
    {
        _mouseOver = false;
    }

    private async Task HandleSvgClick(MouseEventArgs e)
    {
        if (!EnableManualTargeting) return;

        try
        {
            await JSRuntime.InvokeVoidAsync("playAudio", "/sounds/throw.mp3");

            var coords = await JSRuntime.InvokeAsync<SvgCoordinates>(
                "dartboardInterop.getSvgCoordinates",
                _svgElementRef,
                e.ClientX,
                e.ClientY);

            await OnManualTargetSelected.InvokeAsync((coords.X, coords.Y));

            await Task.Delay(300);
            await JSRuntime.InvokeVoidAsync("playAudio", "/sounds/impact.mp3");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling SVG click: {ex.Message}");
        }
    }

#pragma warning disable CA1812, S1144
    private sealed class SvgCoordinates
    {
        public double X { get; set; }

        public double Y { get; set; }
    }
#pragma warning restore S1144, CA1812
}
