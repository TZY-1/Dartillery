namespace Dartillery.Core.Models;

/// <summary>
/// Represents a dart player's throwing characteristics and skill profile.
/// Immutable record for thread-safety and value semantics.
/// </summary>
public sealed record PlayerProfile
{
    /// <summary>
    /// Initializes a player profile with default moderate-skill values (equivalent to <see cref="Amateur()"/>).
    /// </summary>
    public PlayerProfile()
    {
    }

    /// <summary>
    /// Player identifier (e.g., name or ID).
    /// </summary>
    public string Name { get; init; } = "Anonymous";

    /// <summary>
    /// Base skill level - standard deviation for release noise.
    /// Lower = more accurate. Range: 0.015 (world-class) to 0.15 (beginner).
    /// </summary>
    public double BaseSkill { get; init; } = 0.05;

    /// <summary>
    /// Systematic horizontal bias (positive = right, negative = left).
    /// Represents consistent aiming offset due to stance, grip, or handedness.
    /// Typical range: -0.03 to +0.03 meters.
    /// </summary>
    public double SystematicBiasX { get; init; }

    /// <summary>
    /// Systematic vertical bias (positive = up, negative = down).
    /// Typical range: -0.03 to +0.03 meters.
    /// </summary>
    public double SystematicBiasY { get; init; }

    /// <summary>
    /// Rate at which fatigue accumulates during a session.
    /// 0.0 = no fatigue, 0.01 = moderate, 0.02+ = high fatigue.
    /// Fatigue increases precision (sigma) over time.
    /// </summary>
    public double FatigueRate { get; init; } = 0.005;

    /// <summary>
    /// Resistance to pressure effects (0.0 to 1.0).
    /// 1.0 = immune to pressure, 0.0 = very susceptible.
    /// Affects precision scaling in high-pressure situations.
    /// </summary>
    public double PressureResistance { get; init; } = 0.5;

    /// <summary>
    /// Maximum fatigue magnitude cap (prevents unrealistic degradation).
    /// Added to base skill as session progresses.
    /// </summary>
    public double MaxFatigue { get; init; } = 0.05;

    /// <summary>
    /// Creates a professional-level profile (BaseSkill 0.02, FatigueRate 0.003, PressureResistance 0.8, MaxFatigue 0.03).
    /// </summary>
    /// <param name="name">Player name. Defaults to "Pro".</param>
    /// <returns>A <see cref="PlayerProfile"/> configured for a top-tier player.</returns>
    public static PlayerProfile Professional(string name = "Pro") => new()
    {
        Name = name,
        BaseSkill = 0.02,
        FatigueRate = 0.003,
        PressureResistance = 0.8,
        MaxFatigue = 0.03
    };

    /// <summary>
    /// Creates an amateur-level profile (BaseSkill 0.05, FatigueRate 0.007, PressureResistance 0.5, MaxFatigue 0.05).
    /// </summary>
    /// <param name="name">Player name. Defaults to "Amateur".</param>
    /// <returns>A <see cref="PlayerProfile"/> configured for a recreational club player.</returns>
    public static PlayerProfile Amateur(string name = "Amateur") => new()
    {
        Name = name,
        BaseSkill = 0.05,
        FatigueRate = 0.007,
        PressureResistance = 0.5,
        MaxFatigue = 0.05
    };

    /// <summary>
    /// Creates a beginner-level profile (BaseSkill 0.08, FatigueRate 0.01, PressureResistance 0.3, MaxFatigue 0.08).
    /// </summary>
    /// <param name="name">Player name. Defaults to "Beginner".</param>
    /// <returns>A <see cref="PlayerProfile"/> configured for a novice player.</returns>
    public static PlayerProfile Beginner(string name = "Beginner") => new()
    {
        Name = name,
        BaseSkill = 0.08,
        FatigueRate = 0.01,
        PressureResistance = 0.3,
        MaxFatigue = 0.08
    };
}
