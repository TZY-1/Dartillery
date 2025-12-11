using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Session;

/// <summary>
/// Publishes throw events to registered listeners for logging, analysis, or other side effects.
/// Responsible for coordinating event notifications to all registered listeners.
/// </summary>
/// <remarks>
/// This class follows the Observer pattern, notifying all registered listeners
/// when a throw is completed. Listeners can be used for:
/// - Console logging
/// - CSV/file logging
/// - Real-time statistics
/// - UI updates
/// - Analytics/telemetry
/// </remarks>
public sealed class ThrowEventPublisher
{
    private readonly List<IThrowEventListener> _eventListeners;

    /// <summary>
    /// Initializes a new throw event publisher with the specified listeners.
    /// </summary>
    /// <param name="eventListeners">Collection of event listeners to notify. Can be null or empty.</param>
    public ThrowEventPublisher(IEnumerable<IThrowEventListener>? eventListeners = null)
    {
        _eventListeners = eventListeners?.ToList() ?? new List<IThrowEventListener>();
    }

    /// <summary>
    /// Publishes a throw completed event to all registered listeners.
    /// </summary>
    /// <param name="result">The result of the throw.</param>
    /// <param name="context">The context in which the throw was executed.</param>
    /// <param name="profile">The player profile associated with the throw.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="timestamp">The timestamp when the throw was completed.</param>
    /// <remarks>
    /// This method creates a ThrowEvent and notifies all listeners sequentially.
    /// If a listener throws an exception, it will propagate to the caller.
    /// Consider adding error handling if listener failures should not affect throw execution.
    /// </remarks>
    public void Publish(
        ThrowResult result,
        ThrowContext context,
        PlayerProfile profile,
        long sessionId,
        DateTime timestamp)
    {
        var evt = new ThrowEvent
        {
            Result = result,
            Context = context,
            Profile = profile,
            SessionId = sessionId,
            Timestamp = timestamp
        };

        foreach (var listener in _eventListeners)
        {
            listener.OnThrowCompleted(evt);
        }
    }

    /// <summary>
    /// Adds a new event listener to receive throw notifications.
    /// </summary>
    /// <param name="listener">The listener to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when listener is null.</exception>
    public void AddListener(IThrowEventListener listener)
    {
        if (listener == null)
            throw new ArgumentNullException(nameof(listener));

        if (!_eventListeners.Contains(listener))
        {
            _eventListeners.Add(listener);
        }
    }

    /// <summary>
    /// Removes an event listener from receiving throw notifications.
    /// </summary>
    /// <param name="listener">The listener to remove.</param>
    /// <returns>True if the listener was removed; false if it was not found.</returns>
    public bool RemoveListener(IThrowEventListener listener)
    {
        return _eventListeners.Remove(listener);
    }

    /// <summary>
    /// Gets the count of registered event listeners.
    /// </summary>
    public int ListenerCount => _eventListeners.Count;
}
