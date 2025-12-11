using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Strategy;

/// <summary>
/// Adaptive strategy: adjusts target risk based on recent accuracy.
/// Switches between aggressive and conservative play based on performance.
/// </summary>
public sealed class AdaptiveSelector : ITargetSelector
{
    private readonly double _accuracyThreshold;

    /// <summary>
    /// Creates an adaptive selector.
    /// </summary>
    /// <param name="accuracyThreshold">Accuracy threshold to switch strategies (default: 0.5 = 50%).</param>
    public AdaptiveSelector(double accuracyThreshold = 0.5)
    {
        _accuracyThreshold = accuracyThreshold;
    }

    public Target SelectTarget(GameContext context)
    {
        int remaining = context.RemainingScore;
        bool isAccurate = context.RecentAccuracy >= _accuracyThreshold;

        // Always prioritize checkout
        if (remaining <= 40 && remaining % 2 == 0 && remaining >= 2)
        {
            return Target.Double(remaining / 2);
        }

        // Special case: 50 = Bullseye
        if (remaining == 50)
        {
            return Target.Bullseye();
        }

        // Setup for checkout
        if (remaining < 60 && remaining % 2 == 1)
        {
            int targetSingle = remaining - 40;
            if (targetSingle >= 1 && targetSingle <= 20)
            {
                return Target.Single(targetSingle);
            }
            return Target.Single(1);
        }

        // Adaptive behavior based on recent accuracy
        if (!isAccurate && remaining > 100)
        {
            // Playing poorly: switch to safer singles for consistency
            return Target.Single(20);
        }

        if (isAccurate)
        {
            // Playing well: go for triples aggressively
            return Target.Triple(20);
        }

        // Moderate accuracy: aim for triples but not maximum risk
        return Target.Triple(19); // Slightly safer than T20
    }
}
