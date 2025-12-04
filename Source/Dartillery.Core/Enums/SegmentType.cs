namespace Dartillery.Core.Enums;

/// <summary>
/// Types of dartboard segments.
/// </summary>
public enum SegmentType
{
    /// <summary>Outside the board - no hit.</summary>
    Miss = 0,

    /// <summary>Single field.</summary>
    Single = 1,

    /// <summary>Double ring (outer).</summary>
    Double = 2,

    /// <summary>Triple ring.</summary>
    Triple = 3,

    /// <summary>Outer bull (Single Bull) - 25 points.</summary>
    OuterBull = 4,

    /// <summary>Inner bull (Double Bull / Bullseye) - 50 points.</summary>
    InnerBull = 5
}
