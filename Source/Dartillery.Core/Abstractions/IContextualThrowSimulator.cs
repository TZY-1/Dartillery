using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Extended throw simulator with context support.
/// Separate interface to maintain backward compatibility with IThrowSimulator.
/// </summary>
public interface IContextualThrowSimulator
{
    /// <summary>
    /// Executes a throw with full context awareness.
    /// </summary>
    /// <param name="target">Target to aim at.</param>
    /// <param name="context">Throw context with session state and modifiers.</param>
    /// <returns>The result of the throw.</returns>
    ThrowResult Throw(Target target, ThrowContext context);
}
