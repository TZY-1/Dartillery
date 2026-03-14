using Dartillery.Core.Models;

namespace Dartillery.Core.Abstractions;

/// <summary>
/// Observer pattern for throw events.
/// Enables logging, analytics, and bot training data collection.
/// </summary>
public interface IThrowEventListener
{
    /// <summary>
    /// Called when a throw has been completed.
    /// </summary>
    /// <param name="evt">Event data with result, context, and player info.</param>
    void OnThrowCompleted(ThrowEvent evt);
}
