using Dartillery.Core.Models;
using Dartillery.Web.Models;

namespace Dartillery.Web.Services;

/// <summary>
/// Scoped service for managing a player's simulation session lifecycle.
/// </summary>
public sealed class SimulationService
{
    private readonly List<ThrowResult> _throws = new();

    private PlayerSession? _session;

    /// <summary>Raised when any simulation state changes.</summary>
#pragma warning disable CA1003 // Blazor UI notification pattern uses Action, not EventHandler
    public event Action? OnStateChanged;
#pragma warning restore CA1003

    /// <summary>Selected skill level preset name.</summary>
    public string SelectedSkillLevel { get; set; } = "Amateur";

    /// <summary>Base precision (sigma) for throw deviation.</summary>
    public double CustomSigma { get; set; } = 0.05;

    /// <summary>Whether fatigue simulation is active.</summary>
    public bool EnableFatigue { get; set; } = true;

    /// <summary>Whether pressure simulation is active.</summary>
    public bool EnablePressure { get; set; }

    /// <summary>Active pressure scenario for game context.</summary>
    public PressureScenario PressureScenario { get; set; } = PressureScenarios.Relaxed;

    /// <summary>Whether bust rules are enforced during pressure.</summary>
    public bool PressureEnforceBust { get; set; } = true;

    /// <summary>Fatigue accumulation rate per throw.</summary>
    public double FatigueRate { get; set; } = 0.007;

    /// <summary>Resistance to pressure effects (0–1).</summary>
    public double PressureResistance { get; set; } = 0.5;

    /// <summary>Maximum fatigue cap.</summary>
    public double MaxFatigue { get; set; } = 0.05;

    /// <summary>Selected fatigue model type.</summary>
    public FatigueModelType FatigueModelType { get; set; } = FatigueModelType.Logarithmic;

    /// <summary>Growth rate for logarithmic fatigue model.</summary>
    public double FatigueGrowthRate { get; set; } = 0.01;

    /// <summary>Whether momentum (hot/cold streak) is active.</summary>
    public bool EnableMomentum { get; set; }

    /// <summary>Number of recent throws to analyze for momentum.</summary>
    public int MomentumWindowSize { get; set; } = 6;

    /// <summary>Success rate threshold to trigger hot streak.</summary>
    public double MomentumHotThreshold { get; set; } = 0.7;

    /// <summary>Failure rate threshold to trigger cold streak.</summary>
    public double MomentumColdThreshold { get; set; } = 0.5;

    /// <summary>Deviation reduction bonus during hot streak.</summary>
    public double MomentumHotBonus { get; set; } = 0.05;

    /// <summary>Deviation increase penalty during cold streak.</summary>
    public double MomentumColdPenalty { get; set; } = 0.1;

    /// <summary>Deviation factor threshold for classifying a "good" throw.</summary>
    public double MomentumGoodDeviation { get; set; } = 1.0;

    /// <summary>Deviation factor threshold for classifying a "bad" throw.</summary>
    public double MomentumBadDeviation { get; set; } = 2.5;

    /// <summary>Whether dart grouping/deflection is active.</summary>
    public bool EnableGrouping { get; set; }

    /// <summary>Distance within which darts deflect each other.</summary>
    public double GroupingClusterRadius { get; set; } = 0.08;

    /// <summary>Maximum deflection offset when darts overlap.</summary>
    public double GroupingMaxDeflection { get; set; } = 0.04;

    /// <summary>Whether target-specific difficulty scaling is active.</summary>
    public bool EnableTargetDifficulty { get; set; }

    /// <summary>Whether clicking the dartboard triggers manual throws.</summary>
    public bool EnableManualTargeting { get; set; }

    /// <summary>Selected spread algorithm (Gaussian, Uniform, or Bivariate).</summary>
    public SpreadMode SpreadMode { get; set; } = SpreadMode.Gaussian;

    /// <summary>Vertical-to-horizontal sigma ratio for bivariate mode.</summary>
    public double BivariateSigmaRatio { get; set; } = 0.7;

    /// <summary>Rotation angle in degrees for the bivariate spread ellipse.</summary>
    public double BivariateAngleDegrees { get; set; }

    /// <summary>Throw-to-throw consistency for bivariate mode (0–1).</summary>
    public double BivariateConsistency { get; set; } = 0.8;

    /// <summary>Whether to show the spread shape overlay on the board.</summary>
    public bool ShowSpreadCircle { get; set; } = true;

    /// <summary>Base spread bounds (without fatigue) for visualization.</summary>
    public ISpreadBounds? BaseBounds { get; private set; }

    /// <summary>Effective spread bounds (with fatigue) for visualization.</summary>
    public ISpreadBounds? EffectiveBounds { get; private set; }

    /// <summary>All throw results in the current session.</summary>
    public IReadOnlyList<ThrowResult> Throws => _throws.AsReadOnly();

    /// <summary>Current accumulated fatigue value.</summary>
    public double CurrentFatigue => _session?.CurrentFatigue ?? 0.0;

    /// <summary>Total throws in the current session.</summary>
    public int ThrowCount => _session?.ThrowCount ?? 0;

    /// <summary>Active player name.</summary>
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
    /// Exposes the game state tracker for pressure visualization in UI components.
    /// </summary>
    public GameStateTracker GameState { get; } = new();

    /// <summary>
    /// Returns the pressure modifier that would apply to the next throw based on current game state.
    /// </summary>
    public double PreviewPressureModifier =>
        EnablePressure && _session != null
            ? _session.PreviewPressureModifier(GameState.ToGameContext())
            : 1.0;

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

        if (SpreadMode == SpreadMode.Bivariate)
        {
            builder.WithBivariateParameters(BivariateSigmaRatio, BivariateAngleDegrees, BivariateConsistency);
        }

        builder.WithTruncation();

        // Compute bounds for visualization using the spread algorithm's own logic
        BaseBounds = SpreadBoundsFactory.Create(SpreadMode, CustomSigma, BivariateSigmaRatio, BivariateAngleDegrees);
        var effectivePrecision = CustomSigma + (EnableFatigue ? MaxFatigue : 0);
        EffectiveBounds = SpreadBoundsFactory.Create(SpreadMode, effectivePrecision, BivariateSigmaRatio, BivariateAngleDegrees);

        _session = builder.BuildSession();
        _throws.Clear();
        ResetGameState();

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

        // Bivariate defaults per skill level
        (BivariateSigmaRatio, BivariateAngleDegrees, BivariateConsistency) = preset switch
        {
            "Professional" => (0.85, 5.0, 0.9),
            "Beginner" => (0.5, 15.0, 0.5),
            _ => (0.7, 10.0, 0.7) // Amateur
        };

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
        if (_session == null)
        {
            RebuildSession();
        }

        gameContext ??= BuildGameContext();

        var result = _session!.Throw(target, gameContext);
        _throws.Add(result);

        if (EnablePressure)
        {
            GameState.RecordThrow(result);
        }

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
        ResetGameState();

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

        gameContext ??= BuildGameContext();

        var result = _session!.ThrowAtPoint(new Point2D(x, y), gameContext);
        _throws.Add(result);

        if (EnablePressure)
        {
            GameState.RecordThrow(result);
        }

        NotifyStateChanged();
        return result;
    }

    /// <summary>
    /// Initializes the game state tracker with the current pressure scenario.
    /// Called when pressure settings change or throws are cleared.
    /// </summary>
    public void ResetGameState()
    {
        GameState.EnforceBustRules = PressureEnforceBust;
        GameState.StartScenario(PressureScenario);
    }

    /// <summary>
    /// Builds the appropriate <see cref="GameContext"/> for the next throw.
    /// Uses the dynamic game state tracker when pressure is enabled,
    /// otherwise falls back to a neutral context with visit tracking.
    /// </summary>
    private GameContext BuildGameContext()
    {
        if (EnablePressure)
        {
            GameState.EnforceBustRules = PressureEnforceBust;
            return GameState.ToGameContext();
        }

        // Non-pressure path: basic visit tracking for grouping model
        return new GameContext
        {
            CurrentVisitResults = GetCurrentVisitResults()
        };
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
