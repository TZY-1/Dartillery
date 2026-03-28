using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.FatigueModels;

/// <summary>
/// Time-based fatigue model (not throw-based).
/// Fatigue accumulates based on actual session duration.
/// More realistic for long practice sessions.
/// </summary>
internal sealed class TimeBasedFatigueModel : IFatigueModel
{
    private readonly double _fatiguePerMinute;

    /// <summary>
    /// Creates a time-based fatigue model.
    /// </summary>
    /// <param name="fatiguePerMinute">Fatigue accumulation per minute (default: 0.002).</param>
    public TimeBasedFatigueModel(double fatiguePerMinute = 0.002)
    {
        _fatiguePerMinute = fatiguePerMinute;
    }

    /// <inheritdoc/>
    public double CalculateFatigue(SessionState state, PlayerProfile profile)
    {
        double minutes = state.SessionDuration.TotalMinutes;
        double fatigue = minutes * _fatiguePerMinute * profile.FatigueRate;
        return Math.Min(fatigue, profile.MaxFatigue);
    }
}
