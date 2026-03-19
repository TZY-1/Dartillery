using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Web.Services;

/// <summary>
/// Scoped service for managing a player's simulation session lifecycle.
/// </summary>
public sealed class SimulationService
{
    private readonly ITargetResolver _targetResolver;

    private readonly List<ThrowResult> _throws = new();

    private PlayerSession? _session;

    public SimulationService(ITargetResolver targetResolver)
    {
        this._targetResolver = targetResolver;
    }

#pragma warning disable CA1003 // Blazor UI notification pattern uses Action, not EventHandler
    public event Action? OnStateChanged;
#pragma warning restore CA1003

    // Configuration properties for UI binding
    public string SelectedSkillLevel { get; set; } = "Amateur";

    public double CustomSigma { get; set; } = 0.05;

    public bool EnableFatigue { get; set; } = true;

    public bool EnablePressure { get; set; }

    public double FatigueRate { get; set; } = 0.005;

    public bool EnableManualTargeting { get; set; }

    // Current state properties
    public IReadOnlyList<ThrowResult> Throws => _throws.AsReadOnly();

    public double CurrentTremor => _session?.CurrentTremor ?? 0.0;

    public int ThrowCount => _session?.ThrowCount ?? 0;

    public string PlayerName => _session?.Profile.Name ?? "Unknown";

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

        // Apply skill level preset or custom profile
        builder = SelectedSkillLevel switch
        {
            "Professional" => builder.WithProfessionalPlayer(),
            "Amateur" => builder.WithAmateurPlayer(),
            "Beginner" => builder.WithBeginnerPlayer(),
            "Custom" => builder.WithPlayerProfile(new PlayerProfile
            {
                Name = "Custom",
                BaseSkill = CustomSigma,
                FatigueRate = FatigueRate
            }),
            _ => builder.WithAmateurPlayer()
        };

        // Apply behavioral models based on toggles
        if (EnableFatigue)
        {
            builder.WithRealisticTremor();
        }

        if (EnablePressure)
        {
            builder.WithCheckoutPsychology();
        }

        // Build the session
        _session = builder.BuildSession();
        _throws.Clear();

        NotifyStateChanged();
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

        var result = _session!.Throw(target, gameContext);
        _throws.Add(result);

        NotifyStateChanged();
        return result;
    }

    /// <summary>
    /// Clears all throws but keeps the session intact.
    /// Resets tremor and other accumulated effects.
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
        var target = _targetResolver.Resolve(new Point2D(x, y));

        if (target == null)
            throw new InvalidOperationException("Throwing outside the board is not allowed.");

        return ThrowAt(target, gameContext);
    }

    /// <summary>
    /// Notifies subscribers that state has changed
    /// </summary>
    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
