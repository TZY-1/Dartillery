using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Models inherent difficulty of different targets.
/// Bulls are harder than triples, some sectors harder to see, etc.
/// </summary>
public interface ITargetDifficultyModel
{
    /// <summary>
    /// Returns difficulty modifier for given target.
    /// </summary>
    /// <param name="target">Target being aimed at.</param>
    /// <returns>Difficulty multiplier (> 1.0 = harder/more deviation, < 1.0 = easier, 1.0 = standard).</returns>
    double GetDifficultyModifier(Target target);
}
