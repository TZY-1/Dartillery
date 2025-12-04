using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Interface for simulating dart throws at dartboard targets.
/// </summary>
public interface IThrowSimulator
{
    /// <summary>
    /// Simulates a throw at the specified target.
    /// </summary>
    /// <param name="target">The target to aim for (e.g., Triple 20).</param>
    /// <returns>The throw result including actual hit location and score.</returns>
    ThrowResult Throw(Target target);
}
