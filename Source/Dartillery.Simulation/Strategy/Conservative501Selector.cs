using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Strategy;

/// <summary>
/// Conservative 501 strategy: prioritizes safe targets, avoids risky finishes.
/// Aims for high-percentage shots and steady progress.
/// </summary>
public sealed class Conservative501Selector : ITargetSelector
{
    public Target SelectTarget(GameContext context)
    {
        int remaining = context.RemainingScore;

        // Checkout range: use double finish
        if (remaining <= 40 && remaining % 2 == 0 && remaining >= 2)
        {
            return Target.Double(remaining / 2);
        }

        // Special case: 50 = Bullseye
        if (remaining == 50)
        {
            return Target.Bullseye();
        }

        // Near checkout but odd: aim for single to set up double
        // Calculate which single gets us into checkout range (≤ 40)
        if (remaining < 60 && remaining % 2 == 1)
        {
            int targetSingle = remaining - 40; // e.g., 57 - 40 = 17, aim S17 to leave 40
            if (targetSingle >= 1 && targetSingle <= 20)
            {
                return Target.Single(targetSingle);
            }
            // Fallback: aim S1 to leave even
            return Target.Single(1);
        }

        // Default: aim for T20 (highest expected value)
        return Target.Triple(20);
    }
}
