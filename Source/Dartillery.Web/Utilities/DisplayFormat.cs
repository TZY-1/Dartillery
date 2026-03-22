using Dartillery.Shared;

namespace Dartillery.Web.Utilities;

public static class DisplayFormat
{
    private const double _boardRadius = BoardDimensions.DartBoardRadiusInMM;

    /// <summary>"8.5 mm (σ 0.050)" — sigma values with σ prefix in raw</summary>
    public static string Sigma(double sigma) => $"{sigma * _boardRadius:F1} mm (σ {sigma:F4})";

    /// <summary>"+0.4 mm (+0.0023)" — additive deltas</summary>
    public static string Delta(double delta) => $"+{delta * _boardRadius:F1} mm (+{delta:F4})";

    /// <summary>"5.8 mm (0.034)" — general normalized values</summary>
    public static string Mm(double normalized) => $"{normalized * _boardRadius:F1} mm ({normalized:F3})";

    /// <summary>"8.5 mm" — compact, no raw value (for sliders)</summary>
    public static string MmShort(double normalized) => $"{normalized * _boardRadius:F1} mm";

    /// <summary>Converts normalized board coordinate to millimeters.</summary>
    public static double ToMm(double normalized) => normalized * _boardRadius;
}
