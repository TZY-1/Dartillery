using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Session;

/// <summary>
/// Dispatches <see cref="ThrowEvent"/> notifications to all registered <see cref="IThrowEventListener"/> instances after each throw.
/// All listeners are invoked regardless of individual failures; exceptions are collected and rethrown as <see cref="AggregateException"/>.
/// </summary>
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
    /// Gets the number of currently registered event listeners.
    /// </summary>
    public int ListenerCount => _eventListeners.Count;

    /// <summary>
    /// Constructs a <see cref="ThrowEvent"/> and dispatches it to all registered listeners.
    /// </summary>
    /// <param name="result">The result of the throw.</param>
    /// <param name="context">The context in which the throw was executed.</param>
    /// <param name="profile">The player profile associated with the throw.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="timestamp">The timestamp when the throw was completed.</param>
    /// <exception cref="AggregateException">Thrown when one or more listeners throw during event notification.</exception>
    public void Publish(
        ThrowResult result,
        ThrowContext context,
        PlayerProfile profile,
        Guid sessionId,
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

        List<Exception>? errors = null;
        foreach (var listener in _eventListeners)
        {
            try
            {
                listener.OnThrowCompleted(evt);
            }
            catch (Exception ex)
            {
                errors ??= new List<Exception>();
                errors.Add(ex);
            }
        }

        if (errors is { Count: > 0 })
            throw new AggregateException("One or more throw event listeners threw an exception.", errors);
    }

    /// <summary>
    /// Adds a new event listener to receive throw notifications.
    /// </summary>
    /// <param name="listener">The listener to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when listener is null.</exception>
    public void AddListener(IThrowEventListener listener)
    {
        ArgumentNullException.ThrowIfNull(listener);

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
}
