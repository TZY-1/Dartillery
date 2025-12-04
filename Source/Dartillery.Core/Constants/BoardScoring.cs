namespace Dartillery.Core.Constants;

/// <summary>
/// Scoring constants for dartboard segments.
/// </summary>
public static class BoardScoring
{
    /// <summary>Score for hitting the inner bull (bullseye).</summary>
    public const int InnerBullScore = 50;

    /// <summary>Score for hitting the outer bull (single bull).</summary>
    public const int OuterBullScore = 25;

    /// <summary>Multiplier for triple ring hits.</summary>
    public const int TripleMultiplier = 3;

    /// <summary>Multiplier for double ring hits.</summary>
    public const int DoubleMultiplier = 2;

    /// <summary>Multiplier for single area hits.</summary>
    public const int SingleMultiplier = 1;
}
