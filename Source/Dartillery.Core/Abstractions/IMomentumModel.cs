using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Models hot hand / cold streak effects.
/// Tracks recent performance and adjusts precision accordingly.
/// </summary>
public interface IMomentumModel
{
    /// <summary>
    /// Calculates momentum modifier based on recent throw history.
    /// </summary>
    /// <param name="recentHistory">Recent throws (typically last 3-10 throws).</param>
    /// <returns>Momentum multiplier (< 1.0 = hot hand/better, > 1.0 = cold streak/worse, 1.0 = neutral).</returns>
    double CalculateMomentumModifier(IReadOnlyList<ThrowResult> recentHistory);
}
