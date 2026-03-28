using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Simulation.Models.PressureModels;

/// <summary>
/// Advanced pressure model including bogey numbers and score anxiety.
/// Models psychological effects of difficult checkout situations.
/// </summary>
internal sealed class CheckoutPsychologyModel : IPressureModel
{
    private static readonly HashSet<int> _bogeyNumbers = new()
    {
        169, 168, 166, 165, 163, 162, 159,
    };

    /// <inheritdoc/>
    public double GetPrecisionModifier(PlayerProfile profile, GameContext context)
    {
        double pressure = 1.0;

        if (context.IsCheckoutAttempt)
        {
            pressure += 0.05;
            pressure += 0.05 * context.CheckoutAttempts;

            if (context.RemainingScore >= 150)
            {
                pressure += 0.08;
            }

            if (_bogeyNumbers.Contains(context.RemainingScore))
            {
                pressure += 0.04;
            }
        }

        // Last dart in visit during checkout — now-or-never moment
        if (context.ThrowsRemainingInVisit == 1 && context.IsCheckoutAttempt)
        {
            pressure += 0.06;
        }
        else if (context.ThrowsRemainingInVisit == 2 && context.IsCheckoutAttempt)
        {
            pressure += 0.03;
        }

        // Missed darts in the current visit compound frustration
        if (context.CurrentVisitResults.Count > 0 && context.IsCheckoutAttempt)
        {
            int missedDarts = context.CurrentVisitResults.Count(r => r.Score == 0 || !r.IsHit);
            pressure += 0.03 * missedDarts;
        }

        if (context.IsMatchPoint)
        {
            pressure += 0.10;
        }

        if (context.RemainingScore is > 0 and < 40)
        {
            pressure += 0.03;
        }

        return 1.0 + ((pressure - 1.0) * (1.0 - profile.PressureResistance));
    }
}
