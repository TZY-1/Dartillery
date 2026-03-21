namespace Dartillery;

/// <summary>
/// Determines how fatigue accumulates over the course of a session.
/// </summary>
public enum FatigueModelType
{
    /// <summary>Logarithmic curve — fast initial fatigue, then plateau (realistic).</summary>
    Logarithmic,

    /// <summary>Linear accumulation — constant fatigue rate per throw.</summary>
    Linear,

    /// <summary>No fatigue — accuracy never degrades.</summary>
    None
}
