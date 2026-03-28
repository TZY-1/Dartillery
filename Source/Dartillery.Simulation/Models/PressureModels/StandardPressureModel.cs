using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.PressureModels;

/// <summary>
/// Standard pressure model based on checkout attempts and score proximity.
/// </summary>
internal sealed class StandardPressureModel : IPressureModel
{
    /// <inheritdoc/>
    public double GetPrecisionModifier(PlayerProfile profile, GameContext context)
    {
        double basePressure = 1.0;

        // Checkout pressure: increases with attempts
        if (context.IsCheckoutAttempt)
        {
            basePressure += 0.1 * context.CheckoutAttempts;
        }

        // Low score pressure (< 50 remaining)
        if (context.RemainingScore < 50 && context.RemainingScore > 0)
        {
            basePressure += 0.05;
        }

        // Very low score pressure (< 20 remaining)
        if (context.RemainingScore < 20 && context.RemainingScore > 0)
        {
            basePressure += 0.1;
        }

        // Match point pressure
        if (context.IsMatchPoint)
        {
            basePressure += 0.15;
        }

        // Apply player's pressure resistance
        // resistance = 1.0 → no effect, resistance = 0.0 → full pressure
        double finalModifier = 1.0 + ((basePressure - 1.0) * (1.0 - profile.PressureResistance));

        return finalModifier;
    }
}
