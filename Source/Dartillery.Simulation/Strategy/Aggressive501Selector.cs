using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Strategy;

/// <summary>
/// Aggressive 501 strategy: maximizes expected score, takes risks.
/// Goes for triples aggressively but still sets up checkouts intelligently.
/// </summary>
public sealed class Aggressive501Selector : ITargetSelector
{
    public Target SelectTarget(GameContext context)
    {
        int remaining = context.RemainingScore;

        // Always try checkout if possible
        if (remaining <= 40 && remaining % 2 == 0 && remaining >= 2)
        {
            return Target.Double(remaining / 2);
        }

        // Special case: 50 = Bullseye
        if (remaining == 50)
        {
            return Target.Bullseye();
        }

        // In checkout range but odd: set up a checkout
        // Still aggressive - go for triples to leave a double
        if (remaining > 40 && remaining <= 100)
        {
            // Examples:
            // 101 -> T20 (60) leaves 41 (T1 for 38/D19)
            // 61 -> T11 (33) leaves 28 (D14)
            // 51 -> T17 (51) leaves 0 (checkout!) or S11 (40 left/D20)

            // For odd numbers in this range, aim for a triple that leaves a good double
            if (remaining % 2 == 1)
            {
                // Calculate which triple leaves us closest to 32-40 range (ideal checkout)
                int targetForIdealLeave = (remaining - 36) / 3; // Aim to leave ~36 (D18)
                if (targetForIdealLeave >= 1 && targetForIdealLeave <= 20)
                {
                    return Target.Triple(targetForIdealLeave);
                }
                // Fallback: Single to leave even
                return Target.Single(remaining - 40);
            }
        }

        // High score: always go for T20 maximum points
        return Target.Triple(20);
    }
}
