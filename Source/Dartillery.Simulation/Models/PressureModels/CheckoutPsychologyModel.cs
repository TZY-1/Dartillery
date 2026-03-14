using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.PressureModels;

/// <summary>
/// Advanced pressure model including bogey numbers and score anxiety.
/// Models psychological effects of difficult checkout situations.
/// </summary>
internal sealed class CheckoutPsychologyModel : IPressureModel
{
    private static readonly HashSet<int> BogeyNumbers = new()
    {
        169, 168, 166, 165, 163, 162, 159 // Awkward finishes (no easy double-out)
    };

    public double GetPrecisionModifier(PlayerProfile profile, GameContext context)
    {
        double pressure = 1.0;

        // Base checkout pressure
        if (context.IsCheckoutAttempt)
        {
            pressure += 0.1 * context.CheckoutAttempts;

            // High-value checkout pressure (170, 167, etc.)
            if (context.RemainingScore >= 150)
            {
                pressure += 0.15;
            }

            // Bogey number pressure (awkward finishes)
            if (BogeyNumbers.Contains(context.RemainingScore))
            {
                pressure += 0.08;
            }
        }

        // Match point pressure
        if (context.IsMatchPoint)
        {
            pressure += 0.2;
        }

        // Score anxiety (fear of busting)
        if (context.RemainingScore < 40)
        {
            pressure += 0.05; // Nervous about going bust
        }

        // Apply player's pressure resistance
        return 1.0 + (pressure - 1.0) * (1.0 - profile.PressureResistance);
    }
}
