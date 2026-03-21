using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;
using Dartillery.Session;

namespace Dartillery;

/// <summary>
/// Coordinates a player's throwing session: executes throws, manages session state, and publishes events.
/// Obtain instances via <see cref="EnhancedDartboardSimulatorBuilder.BuildSession"/> or <see cref="SimulatorPresets"/>.
/// </summary>
/// <remarks>This type is not thread-safe. Create separate instances for concurrent use.</remarks>
public sealed class PlayerSession
{
    private readonly IContextualThrowSimulator _simulator;
    private readonly SessionStateManager _stateManager;
    private readonly ThrowContextBuilder _contextBuilder;
    private readonly ThrowEventPublisher _eventPublisher;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new player session with the specified simulator and behavioral models.
    /// </summary>
    /// <param name="simulator">The contextual throw simulator for executing throws.</param>
    /// <param name="profile">The player profile containing skill characteristics.</param>
    /// <param name="fatigueModel">The fatigue model for accuracy degradation.</param>
    /// <param name="pressureModel">The pressure model for psychological effects.</param>
    /// <param name="momentumModel">The momentum model for streak effects.</param>
    /// <param name="groupingModel">The grouping model for dart clustering.</param>
    /// <param name="targetDifficultyModel">The difficulty model for target-specific challenges.</param>
    /// <param name="eventListeners">Optional collection of event listeners for throw notifications.</param>
    /// <param name="timeProvider">Optional time provider for testability. Defaults to <see cref="TimeProvider.System"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="simulator"/> or <paramref name="profile"/> is null.</exception>
    public PlayerSession(
        IContextualThrowSimulator simulator,
        PlayerProfile profile,
        IFatigueModel fatigueModel,
        IPressureModel pressureModel,
        IMomentumModel momentumModel,
        IGroupingModel groupingModel,
        ITargetDifficultyModel targetDifficultyModel,
        IEnumerable<IThrowEventListener>? eventListeners = null,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(simulator);
        ArgumentNullException.ThrowIfNull(profile);
        _simulator = simulator;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _stateManager = new SessionStateManager(profile, _timeProvider);

        _contextBuilder = new ThrowContextBuilder(
            profile,
            fatigueModel,
            pressureModel,
            momentumModel,
            groupingModel,
            targetDifficultyModel);

        _eventPublisher = new ThrowEventPublisher(eventListeners);
    }

    /// <summary>
    /// All throw results in chronological order. Cleared on <see cref="Reset"/>.
    /// </summary>
    public IReadOnlyList<ThrowResult> ThrowHistory => _stateManager.ThrowHistory;

    /// <summary>
    /// Total throws executed in this session. Resets to zero on <see cref="Reset"/>.
    /// </summary>
    public int ThrowCount => _stateManager.ThrowCount;

    /// <summary>
    /// Current accumulated fatigue magnitude as calculated by the configured fatigue model.
    /// Increases over the session based on throw count and fatigue rate; resets to zero on <see cref="Reset"/>.
    /// </summary>
    public double CurrentFatigue => _contextBuilder.CurrentFatigue;

    /// <summary>
    /// The player profile defining skill characteristics for this session.
    /// </summary>
    public PlayerProfile Profile => _stateManager.Profile;

    /// <summary>
    /// Unique identifier for this session instance, generated at construction time.
    /// Used to correlate throw events across multiple listeners.
    /// </summary>
    public Guid SessionId => _stateManager.SessionId;

    /// <summary>
    /// Executes a dart throw at the specified target, applying all configured behavioral modifiers.
    /// </summary>
    /// <param name="target">The target segment to aim at (e.g., Triple 20, Double 16, Bullseye).</param>
    /// <param name="gameContext">
    /// Optional game context for pressure calculations (remaining score, checkout status, visit history).
    /// When null, pressure modifier is 1.0 (neutral).
    /// </param>
    /// <returns>A <see cref="ThrowResult"/> containing the scored segment, points, and hit coordinates.</returns>
    public ThrowResult Throw(Target target, GameContext? gameContext = null)
    {
        var sessionState = _stateManager.GetCurrentState();
        var context = _contextBuilder.BuildContext(sessionState, _stateManager.ThrowHistory, gameContext);
        var result = _simulator.Throw(target, context);
        _stateManager.RecordThrow(result);
        _eventPublisher.Publish(
            result,
            context,
            _stateManager.Profile,
            _stateManager.SessionId,
            _timeProvider.GetUtcNow().UtcDateTime);

        return result;
    }

    /// <summary>
    /// Executes a dart throw aimed at exact coordinates instead of a target segment center.
    /// </summary>
    public ThrowResult ThrowAtPoint(Point2D aimPoint, GameContext? gameContext = null)
    {
        var sessionState = _stateManager.GetCurrentState();
        var context = _contextBuilder.BuildContext(sessionState, _stateManager.ThrowHistory, gameContext);
        var result = _simulator.ThrowAtPoint(aimPoint, context);
        _stateManager.RecordThrow(result);
        _eventPublisher.Publish(
            result,
            context,
            _stateManager.Profile,
            _stateManager.SessionId,
            _timeProvider.GetUtcNow().UtcDateTime);

        return result;
    }

    /// <summary>
    /// Resets the session to initial state: clears throw history, resets throw count and fatigue accumulation.
    /// The player profile and behavioral models are preserved.
    /// </summary>
    public void Reset()
    {
        _stateManager.Reset();
        _contextBuilder.Reset();
    }
}
