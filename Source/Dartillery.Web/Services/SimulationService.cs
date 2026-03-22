using Dartillery.Core.Models;

namespace Dartillery.Web.Services;

/// <summary>
/// Scoped service for managing a player's simulation session lifecycle.
/// </summary>
public sealed class SimulationService
{
    private readonly List<ThrowResult> _throws = new();

    private PlayerSession? _session;

#pragma warning disable CA1003 // Blazor UI notification pattern uses Action, not EventHandler
    public event Action? OnStateChanged;
#pragma warning restore CA1003

    // Configuration properties for UI binding
    public string SelectedSkillLevel { get; set; } = "Amateur";

    public double CustomSigma { get; set; } = 0.05;

    public bool EnableFatigue { get; set; } = true;

    public bool EnablePressure { get; set; }

    public double FatigueRate { get; set; } = 0.007;

    public double PressureResistance { get; set; } = 0.5;

    public double MaxFatigue { get; set; } = 0.05;

    public FatigueModelType FatigueModelType { get; set; } = FatigueModelType.Logarithmic;

    public double FatigueGrowthRate { get; set; } = 0.01;

    public bool EnableMomentum { get; set; }

    public int MomentumWindowSize { get; set; } = 6;

    public double MomentumHotThreshold { get; set; } = 0.7;

    public double MomentumColdThreshold { get; set; } = 0.5;

    public double MomentumHotBonus { get; set; } = 0.05;

    public double MomentumColdPenalty { get; set; } = 0.1;

    public double MomentumGoodDeviation { get; set; } = 1.0;

    public double MomentumBadDeviation { get; set; } = 2.5;

    public bool EnableGrouping { get; set; }

    public double GroupingClusterRadius { get; set; } = 0.08;

    public double GroupingMaxDeflection { get; set; } = 0.04;

    public bool EnableTargetDifficulty { get; set; }

    public bool EnableManualTargeting { get; set; }

    public SpreadMode SpreadMode { get; set; } = SpreadMode.Gaussian;

    public bool ShowSpreadCircle { get; set; } = true;

    public ISpreadBounds? BaseBounds { get; private set; }

    public ISpreadBounds? EffectiveBounds { get; private set; }

    // Current state properties
    public IReadOnlyList<ThrowResult> Throws => _throws.AsReadOnly();

    public double CurrentFatigue => _session?.CurrentFatigue ?? 0.0;

    public int ThrowCount => _session?.ThrowCount ?? 0;

    public string PlayerName => _session?.Profile.Name ?? "Unknown";

    /// <summary>
    /// Exposes the active player profile for visualization components.
    /// </summary>
    public PlayerProfile? CurrentProfile => _session?.Profile;

    /// <summary>
    /// Calculates average score from all throws
    /// </summary>
    public double AverageScore =>
        _throws.Count > 0 ? _throws.Average(t => t.Score) : 0.0;

    /// <summary>
    /// Calculates hit rate (percentage of throws that hit the board)
    /// </summary>
    public double HitRate =>
        _throws.Count > 0 ? _throws.Count(t => t.IsHit) / (double)_throws.Count : 0.0;

    /// <summary>
    /// Gets the most recent throw result, or null if no throws yet
    /// </summary>
    public ThrowResult? LastThrow => _throws.Count > 0 ? _throws[^1] : null;

    /// <summary>
    /// Rebuilds the session with current configuration settings.
    /// Called when parameters change to apply new settings.
    /// </summary>
    public void RebuildSession()
    {
        var builder = new EnhancedDartboardSimulatorBuilder();

        // Always build from current property values
        builder.WithPlayerProfile(new PlayerProfile
        {
            Name = SelectedSkillLevel,
            BaseSkill = CustomSigma,
            FatigueRate = FatigueRate,
            PressureResistance = PressureResistance,
            MaxFatigue = MaxFatigue
        });

        // Apply behavioral models based on toggles
        if (EnableFatigue)
        {
            switch (FatigueModelType)
            {
                case FatigueModelType.Logarithmic:
                    builder.WithRealisticFatigue(FatigueGrowthRate);
                    break;
                case FatigueModelType.Linear:
                    builder.WithLinearFatigue();
                    break;
                case FatigueModelType.None:
                    builder.WithNoFatigue();
                    break;
            }
        }
        else
        {
            builder.WithNoFatigue();
        }

        if (EnablePressure)
        {
            builder.WithCheckoutPsychology();
        }

        if (EnableMomentum)
        {
            builder.WithStandardMomentum(
                MomentumWindowSize, MomentumHotBonus, MomentumColdPenalty,
                MomentumHotThreshold, MomentumColdThreshold,
                MomentumGoodDeviation, MomentumBadDeviation);
        }

        if (EnableGrouping)
        {
            builder.WithSimpleGrouping(GroupingClusterRadius, GroupingMaxDeflection);
        }

        if (EnableTargetDifficulty)
        {
            builder.WithStandardTargetDifficulty();
        }

        builder.WithSpreadMode(SpreadMode);
        builder.WithTruncation();

        // Compute bounds for visualization using the spread algorithm's own logic
        BaseBounds = SpreadBoundsFactory.Create(SpreadMode, CustomSigma);
        var effectivePrecision = CustomSigma + (EnableFatigue ? MaxFatigue : 0);
        EffectiveBounds = SpreadBoundsFactory.Create(SpreadMode, effectivePrecision);

        // Build the session
        _session = builder.BuildSession();
        _throws.Clear();

        NotifyStateChanged();
    }

    /// <summary>
    /// Loads a preset profile's values into the individual properties and rebuilds the session.
    /// </summary>
    public void ApplyPreset(string preset)
    {
        var profile = preset switch
        {
            "Professional" => PlayerProfile.Professional(),
            "Beginner" => PlayerProfile.Beginner(),
            _ => PlayerProfile.Amateur()
        };
        SelectedSkillLevel = preset;
        CustomSigma = profile.BaseSkill;
        FatigueRate = profile.FatigueRate;
        PressureResistance = profile.PressureResistance;
        MaxFatigue = profile.MaxFatigue;
        RebuildSession();
    }

    /// <summary>
    /// Executes a dart throw at the specified target.
    /// Ensures session exists before throwing.
    /// </summary>
    /// <param name="target">The target to aim at</param>
    /// <param name="gameContext">Optional game context for pressure simulation</param>
    /// <returns>The throw result</returns>
    public ThrowResult ThrowAt(Target target, GameContext? gameContext = null)
    {
        // Lazy initialization - rebuild session if not yet created
        if (_session == null)
        {
            RebuildSession();
        }

        // Build visit-aware game context for grouping model
        gameContext ??= new GameContext();
        gameContext = gameContext with
        {
            CurrentVisitResults = GetCurrentVisitResults()
        };

        var result = _session!.Throw(target, gameContext);
        _throws.Add(result);

        NotifyStateChanged();
        return result;
    }

    /// <summary>
    /// Clears all throws but keeps the session intact.
    /// Resets fatigue and other accumulated effects.
    /// </summary>
    public void ClearThrows()
    {
        _throws.Clear();
        _session?.Reset();

        NotifyStateChanged();
    }

    /// <summary>
    /// Clears throws and rebuilds the session with current settings.
    /// </summary>
    public void ResetSession()
    {
        _throws.Clear();
        RebuildSession();
    }

    /// <summary>
    /// Executes a dart throw at specific normalized coordinates (for manual targeting).
    /// Creates a target from the given coordinates and delegates to ThrowAt.
    /// </summary>
    /// <param name="x">Normalized X coordinate (-1.2 to 1.2)</param>
    /// <param name="y">Normalized Y coordinate (-1.2 to 1.2)</param>
    /// <param name="gameContext">Optional game context for pressure simulation</param>
    /// <returns>The throw result</returns>
    public ThrowResult ThrowAtCoordinates(double x, double y, GameContext? gameContext = null)
    {
        if (_session == null)
        {
            RebuildSession();
        }

        // Build visit-aware game context for grouping model
        gameContext ??= new GameContext();
        gameContext = gameContext with
        {
            CurrentVisitResults = GetCurrentVisitResults()
        };

        var result = _session!.ThrowAtPoint(new Point2D(x, y), gameContext);
        _throws.Add(result);

        NotifyStateChanged();
        return result;
    }

    /// <summary>
    /// Returns the throw results from the current 3-dart visit.
    /// Groups throws into visits of 3 darts each.
    /// </summary>
    private List<ThrowResult> GetCurrentVisitResults()
    {
        int dartInVisit = _throws.Count % 3;
        if (dartInVisit == 0)
        {
            return new List<ThrowResult>();
        }

        return _throws.Skip(_throws.Count - dartInVisit).ToList();
    }

    /// <summary>
    /// Notifies subscribers that state has changed
    /// </summary>
    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
