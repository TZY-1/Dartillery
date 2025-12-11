using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.TremorModels;

/// <summary>
/// Time-based tremor model (not throw-based).
/// Fatigue accumulates based on actual session duration.
/// More realistic for long practice sessions.
/// </summary>
public sealed class TimeBasedTremorModel : ITremorModel
{
    private readonly double _fatiguePerMinute;

    /// <summary>
    /// Creates a time-based tremor model.
    /// </summary>
    /// <param name="fatiguePerMinute">Fatigue accumulation per minute (default: 0.002).</param>
    public TimeBasedTremorModel(double fatiguePerMinute = 0.002)
    {
        _fatiguePerMinute = fatiguePerMinute;
    }

    public double CalculateTremor(SessionState state, PlayerProfile profile)
    {
        double minutes = state.SessionDuration.TotalMinutes;
        double tremor = minutes * _fatiguePerMinute * profile.FatigueRate;
        return Math.Min(tremor, profile.MaxTremor);
    }
}
