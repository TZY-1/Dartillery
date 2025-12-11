using Dartillery.Core.Models;

namespace Dartillery.Session;

/// <summary>
/// Manages the state of a player's throwing session including history, counts, and timing.
/// Responsible for tracking throw count, maintaining throw history, and managing session lifecycle.
/// </summary>
/// <remarks>
/// This class handles the Single Responsibility of state management, separated from
/// throw execution and context building for better testability and maintainability.
/// </remarks>
public sealed class SessionStateManager
{
    private readonly PlayerProfile _profile;
    private readonly List<ThrowResult> _throwHistory = new();
    private readonly long _sessionId;

    private int _throwCount = 0;
    private DateTime _sessionStart = DateTime.UtcNow;
    private DateTime _lastThrowTime = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new session state manager for the specified player profile.
    /// </summary>
    /// <param name="profile">The player profile containing player characteristics and skill level.</param>
    /// <exception cref="ArgumentNullException">Thrown when profile is null.</exception>
    public SessionStateManager(PlayerProfile profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _sessionId = GenerateSessionId();
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
    /// Gets the unique identifier for this session.
    /// </summary>
    /// <remarks>
    /// Session ID is generated at session creation time using UTC ticks,
    /// ensuring uniqueness across different sessions.
    /// </remarks>
    public long SessionId => _sessionId;

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
    public TimeSpan TimeSinceLastThrow => DateTime.UtcNow - _lastThrowTime;

    /// <summary>
    /// Records a throw result in the session history.
    /// </summary>
    /// <param name="result">The throw result to record.</param>
    /// <remarks>
    /// This method updates the throw count, adds the result to history,
    /// and updates the last throw timestamp.
    /// </remarks>
    public void RecordThrow(ThrowResult result)
    {
        _throwHistory.Add(result);
        _throwCount++;
        _lastThrowTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a session state snapshot for the current moment.
    /// </summary>
    /// <returns>A SessionState object containing current session metrics.</returns>
    /// <remarks>
    /// Used by tremor models and other time-dependent calculations to get
    /// the current state of the session without exposing internal state directly.
    /// </remarks>
    public SessionState GetCurrentState()
    {
        var now = DateTime.UtcNow;
        return new SessionState
        {
            ThrowCount = _throwCount,
            SessionDuration = now - _sessionStart,
            TimeSinceLastThrow = now - _lastThrowTime,
            CurrentTremor = 0.0 // Will be set by ThrowContextBuilder
        };
    }

    /// <summary>
    /// Resets the session state to initial values, clearing all history and counters.
    /// </summary>
    /// <remarks>
    /// Useful for starting a new game or practice session without creating a new
    /// SessionStateManager instance. Preserves the player profile and generates a new session ID.
    /// </remarks>
    public void Reset()
    {
        _throwCount = 0;
        _throwHistory.Clear();
        _sessionStart = DateTime.UtcNow;
        _lastThrowTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates a unique session identifier based on current UTC ticks.
    /// </summary>
    /// <returns>A unique long value representing the session ID.</returns>
    private static long GenerateSessionId() => DateTime.UtcNow.Ticks;
}
