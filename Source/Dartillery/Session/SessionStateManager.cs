using Dartillery.Core.Models;

namespace Dartillery.Session;

/// <summary>
/// Tracks throw history, counts, and session timing for a single player session.
/// </summary>
public sealed class SessionStateManager
{
    private readonly PlayerProfile _profile;
    private readonly List<ThrowResult> _throwHistory = new();
    private readonly TimeProvider _timeProvider;
    private readonly Guid _sessionId;

    private int _throwCount = 0;
    private DateTime _sessionStart;
    private DateTime _lastThrowTime;

    /// <summary>
    /// Initializes a new session state manager for the specified player profile.
    /// </summary>
    /// <param name="profile">The player profile containing player characteristics and skill level.</param>
    /// <param name="timeProvider">Optional time provider for testability. Defaults to <see cref="TimeProvider.System"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when profile is null.</exception>
    public SessionStateManager(PlayerProfile profile, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        _profile = profile;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _sessionId = Guid.NewGuid();
        _sessionStart = _timeProvider.GetUtcNow().UtcDateTime;
        _lastThrowTime = _sessionStart;
    }

    /// <summary>
    /// Gets the player profile associated with this session.
    /// </summary>
    public PlayerProfile Profile => _profile;

    /// <summary>
    /// Gets the total number of throws made in this session.
    /// </summary>
    public int ThrowCount => _throwCount;

    /// <summary>
    /// Gets the read-only collection of all throw results in chronological order.
    /// </summary>
    public IReadOnlyList<ThrowResult> ThrowHistory => _throwHistory.AsReadOnly();

    /// <summary>
    /// Gets the unique identifier assigned to this session at creation time.
    /// </summary>
    public Guid SessionId => _sessionId;

    /// <summary>
    /// Gets the time when the session was started.
    /// </summary>
    public DateTime SessionStart => _sessionStart;

    /// <summary>
    /// Gets the time of the last throw in this session.
    /// </summary>
    public DateTime LastThrowTime => _lastThrowTime;

    /// <summary>
    /// Gets the duration of the session from start to the last throw.
    /// </summary>
    public TimeSpan SessionDuration => _lastThrowTime - _sessionStart;

    /// <summary>
    /// Gets the time elapsed since the last throw.
    /// </summary>
    public TimeSpan TimeSinceLastThrow => _timeProvider.GetUtcNow().UtcDateTime - _lastThrowTime;

    /// <summary>
    /// Appends a throw result to the session history and updates the throw count and last-throw timestamp.
    /// </summary>
    /// <param name="result">The throw result to record.</param>
    public void RecordThrow(ThrowResult result)
    {
        _throwHistory.Add(result);
        _throwCount++;
        _lastThrowTime = _timeProvider.GetUtcNow().UtcDateTime;
    }

    /// <summary>
    /// Returns a point-in-time snapshot of the current session metrics for use by tremor and other time-dependent models.
    /// </summary>
    /// <returns>A <see cref="SessionState"/> with current throw count, duration, and time since last throw.</returns>
    public SessionState GetCurrentState()
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        return new SessionState
        {
            ThrowCount = _throwCount,
            SessionDuration = now - _sessionStart,
            TimeSinceLastThrow = now - _lastThrowTime,
            CurrentTremor = 0.0 // Will be set by ThrowContextBuilder
        };
    }

    /// <summary>
    /// Clears all throw history and resets counters and timestamps without replacing the player profile.
    /// </summary>
    public void Reset()
    {
        _throwCount = 0;
        _throwHistory.Clear();
        _sessionStart = _timeProvider.GetUtcNow().UtcDateTime;
        _lastThrowTime = _timeProvider.GetUtcNow().UtcDateTime;
    }
}
