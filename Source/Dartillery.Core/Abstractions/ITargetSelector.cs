using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Strategy for selecting dart throw targets based on game state.
/// Enables different playing styles and strategies.
/// </summary>
public interface ITargetSelector
{
    /// <summary>
    /// Selects the optimal target for current game context.
    /// </summary>
    /// <param name="context">Game context with remaining score, checkout state, etc.</param>
    /// <returns>Target to aim for.</returns>
    Target SelectTarget(GameContext context);
}
