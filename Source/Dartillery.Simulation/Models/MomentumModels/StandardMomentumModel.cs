using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.MomentumModels;

/// <summary>
/// Standard momentum model tracking hot hand / cold streak effects.
/// Analyzes recent throw history to detect performance trends.
/// </summary>
public sealed class StandardMomentumModel : IMomentumModel
{
    private readonly int _windowSize;
    private readonly double _hotHandBonus;
    private readonly double _coldStreakPenalty;

    /// <summary>
    /// Creates a standard momentum model.
    /// </summary>
    /// <param name="windowSize">Number of recent throws to analyze (default: 6).</param>
    /// <param name="hotHandBonus">Bonus for hot hand - reduces deviation (default: 0.05 = 5% better).</param>
    /// <param name="coldStreakPenalty">Penalty for cold streak - increases deviation (default: 0.1 = 10% worse).</param>
    public StandardMomentumModel(
        int windowSize = 6,
        double hotHandBonus = 0.05,
        double coldStreakPenalty = 0.1)
    {
        _windowSize = windowSize;
        _hotHandBonus = hotHandBonus;
        _coldStreakPenalty = coldStreakPenalty;
    }

    public double CalculateMomentumModifier(IReadOnlyList<ThrowResult> recentHistory)
    {
        if (recentHistory.Count < 3)
        {
            return 1.0; // Not enough data
        }

        // Take last N throws
        var window = recentHistory.TakeLast(_windowSize).ToList();

        // Count successes (hitting high-value targets)
        int triples = window.Count(r => r.SegmentType == SegmentType.Triple);
        int doubles = window.Count(r => r.SegmentType == SegmentType.Double);
        int bulls = window.Count(r => r.IsBull);
        int misses = window.Count(r => !r.IsHit);

        int successes = triples + doubles + bulls;
        double successRate = successes / (double)window.Count;

        // Hot hand: > 70% success rate
        if (successRate > 0.7)
        {
            return 1.0 - _hotHandBonus; // Less deviation (better)
        }

        // Cold streak: > 50% misses
        if (misses > window.Count / 2)
        {
            return 1.0 + _coldStreakPenalty; // More deviation (worse)
        }

        return 1.0; // Neutral
    }
}
