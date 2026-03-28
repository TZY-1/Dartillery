using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.MomentumModels;

/// <summary>
/// Deviation-based momentum model: measures throw consistency relative to base sigma.
/// Good throws (low deviation) build hot streaks, bad throws (high deviation) trigger cold streaks.
/// </summary>
internal sealed class StandardMomentumModel : IMomentumModel
{
    private readonly double _baseSigma;
    private readonly int _windowSize;
    private readonly double _hotHandBonus;
    private readonly double _coldStreakPenalty;
    private readonly double _hotThreshold;
    private readonly double _coldThreshold;
    private readonly double _goodDeviationFactor;
    private readonly double _badDeviationFactor;

    /// <summary>Initializes a new instance with configurable streak parameters.</summary>
    public StandardMomentumModel(
        double baseSigma,
        int windowSize = 6,
        double hotHandBonus = 0.05,
        double coldStreakPenalty = 0.1,
        double hotThreshold = 0.7,
        double coldThreshold = 0.5,
        double goodDeviationFactor = 1.0,
        double badDeviationFactor = 2.5)
    {
        _baseSigma = baseSigma;
        _windowSize = windowSize;
        _hotHandBonus = hotHandBonus;
        _coldStreakPenalty = coldStreakPenalty;
        _hotThreshold = hotThreshold;
        _coldThreshold = coldThreshold;
        _goodDeviationFactor = goodDeviationFactor;
        _badDeviationFactor = badDeviationFactor;
    }

    /// <inheritdoc/>
    public double CalculateMomentumModifier(IReadOnlyList<ThrowResult> recentHistory)
    {
        if (recentHistory.Count == 0)
        {
            return 1.0;
        }

        var window = recentHistory.TakeLast(_windowSize).ToList();

        double goodThreshold = _baseSigma * _goodDeviationFactor;
        double badThreshold = _baseSigma * _badDeviationFactor;

        int successes = window.Count(r => r.Deviation < goodThreshold);
        int failures = window.Count(r => r.Deviation > badThreshold);

        double successRate = successes / (double)_windowSize;
        double failureRate = failures / (double)_windowSize;

        if (successRate >= _hotThreshold)
        {
            return 1.0 - _hotHandBonus;
        }

        if (failureRate >= _coldThreshold)
        {
            return 1.0 + _coldStreakPenalty;
        }

        return 1.0;
    }
}
