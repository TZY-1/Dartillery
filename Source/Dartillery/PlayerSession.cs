using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;
using Dartillery.Session;
using Dartillery.Simulation.Models.GroupingModels;
using Dartillery.Simulation.Models.MomentumModels;
using Dartillery.Simulation.Models.PressureModels;
using Dartillery.Simulation.Models.TargetDifficultyModels;
using Dartillery.Simulation.Models.TremorModels;

namespace Dartillery;

/// <summary>
/// Coordinates a player's throwing session by orchestrating throw execution,
/// state management, context building, and event publishing.
/// </summary>
/// <remarks>
/// <para>
/// PlayerSession acts as a Facade that coordinates three main responsibilities:
/// </para>
/// <list type="bullet">
/// <item><description><see cref="SessionStateManager"/> - Manages throw history, counts, and timing</description></item>
/// <item><description><see cref="ThrowContextBuilder"/> - Builds throw contexts from behavioral models</description></item>
/// <item><description><see cref="ThrowEventPublisher"/> - Publishes events to registered listeners</description></item>
/// </list>
/// <para>
/// This design follows the Single Responsibility Principle by delegating specific
/// concerns to dedicated components, making the code more testable and maintainable.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var session = new EnhancedDartboardSimulatorBuilder()
///     .WithProfessionalPlayer("Alice")
///     .WithLinearTremor()
///     .WithCheckoutPsychology()
///     .BuildSession();
///
/// var result = session.Throw(Target.Triple(20));
/// Console.WriteLine($"Score: {result.Score}, Tremor: {session.CurrentTremor}");
/// </code>
/// </example>
public sealed class PlayerSession
{
    private readonly IContextualThrowSimulator _simulator;
    private readonly SessionStateManager _stateManager;
    private readonly ThrowContextBuilder _contextBuilder;
    private readonly ThrowEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new player session with the specified simulator and behavioral models.
    /// </summary>
    /// <param name="simulator">The contextual throw simulator for executing throws.</param>
    /// <param name="profile">The player profile containing skill characteristics.</param>
    /// <param name="tremorModel">Optional tremor model for fatigue simulation. Defaults to <see cref="NoTremorModel"/>.</param>
    /// <param name="pressureModel">Optional pressure model for psychological effects. Defaults to <see cref="NoPressureModel"/>.</param>
    /// <param name="momentumModel">Optional momentum model for streak effects. Defaults to <see cref="NoMomentumModel"/>.</param>
    /// <param name="groupingModel">Optional grouping model for dart clustering. Defaults to <see cref="NoGroupingModel"/>.</param>
    /// <param name="targetDifficultyModel">Optional difficulty model for target-specific challenges. Defaults to <see cref="NoTargetDifficultyModel"/>.</param>
    /// <param name="eventListeners">Optional collection of event listeners for throw notifications.</param>
    /// <exception cref="ArgumentNullException">Thrown when simulator or profile is null.</exception>
    /// <remarks>
    /// The constructor creates three internal components:
    /// <list type="number">
    /// <item><description>SessionStateManager to track session state</description></item>
    /// <item><description>ThrowContextBuilder to aggregate behavioral model effects</description></item>
    /// <item><description>ThrowEventPublisher to notify event listeners</description></item>
    /// </list>
    /// </remarks>
    public PlayerSession(
        IContextualThrowSimulator simulator,
        PlayerProfile profile,
        ITremorModel? tremorModel = null,
        IPressureModel? pressureModel = null,
        IMomentumModel? momentumModel = null,
        IGroupingModel? groupingModel = null,
        ITargetDifficultyModel? targetDifficultyModel = null,
        IEnumerable<IThrowEventListener>? eventListeners = null)
    {
        _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));

        if (profile == null)
            throw new ArgumentNullException(nameof(profile));

        _stateManager = new SessionStateManager(profile);

        _contextBuilder = new ThrowContextBuilder(
            profile,
            tremorModel ?? new NoTremorModel(),
            pressureModel ?? new NoPressureModel(),
            momentumModel ?? new NoMomentumModel(),
            groupingModel ?? new NoGroupingModel(),
            targetDifficultyModel ?? new NoTargetDifficultyModel());

        _eventPublisher = new ThrowEventPublisher(eventListeners);
    }

    /// <summary>
    /// Executes a dart throw with full context awareness including tremor, pressure, and momentum effects.
    /// </summary>
    /// <param name="target">The target segment to aim at (e.g., Triple 20, Double 16, Bullseye).</param>
    /// <param name="gameContext">
    /// Optional game context containing game-specific state such as remaining score,
    /// checkout status, and previous throws in the current visit. Required for pressure calculations.
    /// </param>
    /// <returns>
    /// A <see cref="ThrowResult"/> containing the scored segment, points, and hit coordinates.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method orchestrates the complete throw workflow:
    /// </para>
    /// <list type="number">
    /// <item><description>Get current session state from SessionStateManager</description></item>
    /// <item><description>Build throw context from behavioral models via ThrowContextBuilder</description></item>
    /// <item><description>Execute throw using the contextual simulator</description></item>
    /// <item><description>Record result in SessionStateManager</description></item>
    /// <item><description>Publish throw event to all registered listeners</description></item>
    /// </list>
    /// <para>
    /// The throw context includes modifiers for:
    /// - <strong>Tremor</strong>: Accumulated fatigue (increases over session)
    /// - <strong>Pressure</strong>: Psychological effects (higher in checkout situations)
    /// - <strong>Momentum</strong>: Hot/cold streak effects (based on recent performance)
    /// - <strong>Grouping</strong>: Dart clustering effects (previous darts in same visit)
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Simple throw without game context
    /// var result = session.Throw(Target.Triple(20));
    ///
    /// // Throw with checkout pressure
    /// var gameContext = new GameContext
    /// {
    ///     RemainingScore = 40,
    ///     CurrentVisitResults = new List&lt;ThrowResult&gt;()
    /// };
    /// var checkoutResult = session.Throw(Target.Double(20), gameContext);
    /// </code>
    /// </example>
    public ThrowResult Throw(Target target, GameContext? gameContext = null)
    {
        // Step 1: Get current session state
        var sessionState = _stateManager.GetCurrentState();

        // Step 2: Build throw context from all behavioral models
        var context = _contextBuilder.BuildContext(sessionState, _stateManager.ThrowHistory, gameContext);

        // Step 3: Execute throw with simulator
        var result = _simulator.Throw(target, context);

        // Step 4: Record throw in state manager
        _stateManager.RecordThrow(result);

        // Step 5: Publish event to listeners
        _eventPublisher.Publish(
            result,
            context,
            _stateManager.Profile,
            _stateManager.SessionId,
            DateTime.UtcNow);

        return result;
    }

    /// <summary>
    /// Resets the session state to initial values, clearing all history and accumulated effects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is useful for starting a new game or practice session without
    /// creating a new PlayerSession instance.
    /// </para>
    /// <para>
    /// The reset operation:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Clears throw history</description></item>
    /// <item><description>Resets throw count to zero</description></item>
    /// <item><description>Resets tremor accumulation to zero</description></item>
    /// <item><description>Resets session timing (start time, last throw time)</description></item>
    /// <item><description>Preserves the player profile and behavioral models</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Play first game
    /// for (int i = 0; i &lt; 50; i++)
    ///     session.Throw(Target.Triple(20));
    ///
    /// Console.WriteLine($"Game 1 tremor: {session.CurrentTremor}");
    ///
    /// // Start new game with fresh state
    /// session.Reset();
    ///
    /// Console.WriteLine($"After reset tremor: {session.CurrentTremor}"); // 0
    /// </code>
    /// </example>
    public void Reset()
    {
        _stateManager.Reset();
        _contextBuilder.Reset();
    }

    /// <summary>
    /// Gets the read-only collection of all throw results in chronological order.
    /// </summary>
    /// <value>
    /// An <see cref="IReadOnlyList{T}"/> of <see cref="ThrowResult"/> objects,
    /// ordered from first throw to most recent throw in the session.
    /// </value>
    /// <remarks>
    /// The history is cleared when <see cref="Reset"/> is called.
    /// This collection is read-only to prevent external modification of session state.
    /// </remarks>
    public IReadOnlyList<ThrowResult> ThrowHistory => _stateManager.ThrowHistory;

    /// <summary>
    /// Gets the total number of throws executed in this session.
    /// </summary>
    /// <value>
    /// A non-negative integer representing the count of throws.
    /// Resets to zero when <see cref="Reset"/> is called.
    /// </value>
    public int ThrowCount => _stateManager.ThrowCount;

    /// <summary>
    /// Gets the current tremor magnitude as calculated by the tremor model.
    /// </summary>
    /// <value>
    /// A non-negative double value representing accumulated tremor/fatigue.
    /// Typically increases over the session duration based on the tremor model's algorithm.
    /// </value>
    /// <remarks>
    /// <para>
    /// The tremor value is model-dependent:
    /// </para>
    /// <list type="bullet">
    /// <item><description><see cref="LinearTremorModel"/>: Increases linearly with throw count</description></item>
    /// <item><description><see cref="LogarithmicTremorModel"/>: Increases logarithmically (fast at first, then plateaus)</description></item>
    /// <item><description><see cref="NoTremorModel"/>: Always returns zero</description></item>
    /// </list>
    /// </remarks>
    public double CurrentTremor => _contextBuilder.CurrentTremor;

    /// <summary>
    /// Gets the player profile associated with this session.
    /// </summary>
    /// <value>
    /// The <see cref="PlayerProfile"/> containing player characteristics such as
    /// base skill level, systematic biases, fatigue rate, and pressure resistance.
    /// </value>
    /// <remarks>
    /// The profile is immutable and set during session construction.
    /// </remarks>
    public PlayerProfile Profile => _stateManager.Profile;

    /// <summary>
    /// Gets the unique identifier for this session instance.
    /// </summary>
    /// <value>
    /// A unique long value based on UTC ticks at session creation time.
    /// </value>
    /// <remarks>
    /// The session ID is useful for correlating throw events across different listeners
    /// and distinguishing between multiple concurrent sessions.
    /// </remarks>
    public long SessionId => _stateManager.SessionId;
}
