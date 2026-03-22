namespace Dartillery.Shared;

/// <summary>
/// Constants for dartboard geometry.
/// All radii are normalized (0 = center, 1 = outer edge of double ring).
/// </summary>
public static class BoardDimensions
{
    // =========================================================================
    // Normalized radii (relative to double ring outer radius)
    // =========================================================================

    /// <summary>Distance from board center to outer edge of the double ring.</summary>
    public const double DartBoardRadiusInMM = 170.0;

    /// <summary>Radius of the inner bull (double bull / bullseye).</summary>
    public const double InnerBullRadius =
        InnerBullRadiusInMM / DartBoardRadiusInMM; // ≈ 0.03735

    /// <summary>Radius of the outer bull (single bull).</summary>
    public const double OuterBullRadius =
        OuterBullRadiusInMM / DartBoardRadiusInMM; // ≈ 0.09353

    /// <summary>Inner radius of the triple ring.</summary>
    public const double TripleRingInner =
        TripleRingInnerRadiusInMM / DartBoardRadiusInMM; // ≈ 0.62941

    /// <summary>Outer radius of the triple ring.</summary>
    public const double TripleRingOuter =
        TripleRingOuterRadiusInMM / DartBoardRadiusInMM; // ≈ 0.67647

    /// <summary>Inner radius of the double ring.</summary>
    public const double DoubleRingInner =
        DoubleRingInnerRadiusInMM / DartBoardRadiusInMM; // ≈ 0.95294

    /// <summary>Outer radius of the double ring (board outer edge).</summary>
    public const double DoubleRingOuter = 1.0;

    // =========================================================================
    // Board layout
    // =========================================================================

    /// <summary>Total radius of the board.</summary>
    public const double BoardRadius = DoubleRingOuter;

    /// <summary>Number of scoring sectors.</summary>
    public const int SectorCount = 20;

    /// <summary>Angular width of a single sector in radians.</summary>
    public const double SectorAngle = 2.0 * Math.PI / SectorCount;

    /// <summary>
    /// Sector order clockwise, starting at 12 o'clock.
    /// </summary>
    public static readonly int[] SectorOrderClockwise =
    {
        20, 1, 18, 4, 13,
        6, 10, 15, 2, 17,
        3, 19, 7, 16, 8,
        11, 14, 9, 12, 5
    };

    // =========================================================================
    // Reference measurements (millimeters)
    // =========================================================================

    /// <summary>Radius of the inner bull (double bull).</summary>
    private const double InnerBullRadiusInMM = 6.35;

    /// <summary>Radius of the outer bull (single bull).</summary>
    private const double OuterBullRadiusInMM = 15.9;

    /// <summary>Distance from board center to inner edge of the triple ring.</summary>
    private const double TripleRingInnerRadiusInMM = 107.0;

    /// <summary>Distance from board center to outer edge of the triple ring.</summary>
    private const double TripleRingOuterRadiusInMM = 115.0;

    /// <summary>Distance from board center to inner edge of the double ring.</summary>
    private const double DoubleRingInnerRadiusInMM = 162.0;
}
