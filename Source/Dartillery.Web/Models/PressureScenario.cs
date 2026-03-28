using Dartillery.Core.Models;

namespace Dartillery.Web.Models;

/// <summary>
/// Immutable description of a game situation that drives pressure calculations.
/// </summary>
public sealed record PressureScenario(
    string Name,
    string Description,
    int RemainingScore,
    bool IsCheckoutAttempt,
    bool IsMatchPoint,
    int CheckoutAttempts)
{
    /// <summary>
    /// Converts this scenario into a <see cref="GameContext"/> for the simulation engine.
    /// </summary>
    public GameContext ToGameContext() => new()
    {
        RemainingScore = RemainingScore,
        IsCheckoutAttempt = IsCheckoutAttempt,
        IsMatchPoint = IsMatchPoint,
        CheckoutAttempts = CheckoutAttempts
    };
}
