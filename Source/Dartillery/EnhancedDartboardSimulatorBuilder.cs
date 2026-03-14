using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;
using Dartillery.Simulation.Adapters;
using Dartillery.Simulation.Calculators;
using Dartillery.Simulation.Models.GroupingModels;
using Dartillery.Simulation.Models.MomentumModels;
using Dartillery.Simulation.Models.PressureModels;
using Dartillery.Simulation.Models.TargetDifficultyModels;
using Dartillery.Simulation.Models.TremorModels;
using Dartillery.Simulation.Services;
using Dartillery.Simulation.Simulators;

namespace Dartillery;

/// <summary>
/// Fluent builder for creating enhanced dart throw simulators with comprehensive realism features.
/// </summary>
/// <remarks>
/// <para>
/// This builder provides a fluent API for configuring all aspects of the dart throw simulation,
/// including player characteristics, behavioral models, and event listeners.
/// </para>
/// <para>
/// The builder supports configuration of:
/// </para>
/// <list type="bullet">
/// <item><description><strong>Player Profiles</strong>: Professional, Amateur, Beginner, or custom profiles</description></item>
/// <item><description><strong>Tremor Models</strong>: Fatigue simulation (linear, logarithmic, or custom)</description></item>
/// <item><description><strong>Pressure Models</strong>: Psychological effects during high-stakes throws</description></item>
/// <item><description><strong>Momentum Models</strong>: Hot/cold streak effects based on recent performance</description></item>
/// <item><description><strong>Grouping Models</strong>: Dart clustering effects within the same visit</description></item>
/// <item><description><strong>Target Difficulty Models</strong>: Target-specific difficulty adjustments</description></item>
/// <item><description><strong>Deviation Truncation</strong>: Limits for maximum deviation to prevent extreme outliers</description></item>
/// <item><description><strong>Event Listeners</strong>: Observers for throw events (logging, analytics, etc.)</description></item>
/// </list>
/// <para>
/// All configuration is optional - the builder provides sensible defaults for any unconfigured options.
/// The final <see cref="BuildSession"/> method constructs a <see cref="PlayerSession"/> with all
/// configured components properly wired together.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Simple professional player with linear tremor
/// var session = new EnhancedDartboardSimulatorBuilder()
///     .WithProfessionalPlayer("Alice")
///     .WithLinearTremor()
///     .BuildSession();
///
/// // Full-featured simulation with all behavioral models
/// var advancedSession = new EnhancedDartboardSimulatorBuilder()
///     .WithAmateurPlayer("Bob")
///     .WithRealisticTremor()
///     .WithCheckoutPsychology()
///     .WithStandardMomentum()
///     .WithSimpleGrouping()
///     .WithStandardTargetDifficulty()
///     .WithTruncation(0.3)
///     .AddEventListener(new ConsoleLogger())
///     .BuildSession();
/// </code>
/// </example>
public sealed class EnhancedDartboardSimulatorBuilder
{
    private PlayerProfile? _profile;
    private ITremorModel? _tremorModel;
    private IPressureModel? _pressureModel;
    private IMomentumModel? _momentumModel;
    private IGroupingModel? _groupingModel;
    private ITargetDifficultyModel? _targetDifficultyModel;
    private IDeviationCalculator? _baseDeviationCalculator;
    private bool _useTruncation = false;
    private double _maxDeviation = 0.25;
    private int? _seed;
    private readonly List<IThrowEventListener> _eventListeners = new();

    /// <summary>
    /// Configures the builder with a custom player profile.
    /// </summary>
    /// <param name="profile">The player profile defining skill characteristics and biases.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// The player profile contains fundamental characteristics such as base skill level,
    /// systematic biases (vertical/horizontal), fatigue rate, and pressure resistance.
    /// Use this method when you need fine-grained control over player characteristics,
    /// or use convenience methods like <see cref="WithProfessionalPlayer"/>,
    /// <see cref="WithAmateurPlayer"/>, or <see cref="WithBeginnerPlayer"/> for preset profiles.
    /// </remarks>
    /// <example>
    /// <code>
    /// var customProfile = new PlayerProfile
    /// {
    ///     Name = "CustomPlayer",
    ///     BaseSkillLevel = 0.015,
    ///     VerticalBias = -0.002,
    ///     HorizontalBias = 0.001,
    ///     FatigueRate = 0.0001,
    ///     PressureResistance = 0.8
    /// };
    ///
    /// var session = new EnhancedDartboardSimulatorBuilder()
    ///     .WithPlayerProfile(customProfile)
    ///     .BuildSession();
    /// </code>
    /// </example>
    public EnhancedDartboardSimulatorBuilder WithPlayerProfile(PlayerProfile profile)
    {
        _profile = profile;
        return this;
    }

    /// <summary>
    /// Configures the builder with a professional-level player profile.
    /// </summary>
    /// <param name="name">The name of the professional player. Defaults to "Pro".</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// Professional players have:
    /// <list type="bullet">
    /// <item><description>Very high accuracy (low base skill level variance)</description></item>
    /// <item><description>Minimal systematic biases</description></item>
    /// <item><description>Low fatigue rate</description></item>
    /// <item><description>High pressure resistance</description></item>
    /// </list>
    /// This preset is suitable for simulating top-tier darts players.
    /// </remarks>
    public EnhancedDartboardSimulatorBuilder WithProfessionalPlayer(string name = "Pro")
    {
        _profile = PlayerProfile.Professional(name);
        return this;
    }

    /// <summary>
    /// Configures the builder with an amateur-level player profile.
    /// </summary>
    /// <param name="name">The name of the amateur player. Defaults to "Amateur".</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// Amateur players have:
    /// <list type="bullet">
    /// <item><description>Moderate accuracy (medium base skill level variance)</description></item>
    /// <item><description>Noticeable systematic biases</description></item>
    /// <item><description>Moderate fatigue rate</description></item>
    /// <item><description>Moderate pressure resistance</description></item>
    /// </list>
    /// This preset represents experienced recreational players.
    /// </remarks>
    public EnhancedDartboardSimulatorBuilder WithAmateurPlayer(string name = "Amateur")
    {
        _profile = PlayerProfile.Amateur(name);
        return this;
    }

    /// <summary>
    /// Configures the builder with a beginner-level player profile.
    /// </summary>
    /// <param name="name">The name of the beginner player. Defaults to "Beginner".</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// Beginner players have:
    /// <list type="bullet">
    /// <item><description>Low accuracy (high base skill level variance)</description></item>
    /// <item><description>Significant systematic biases</description></item>
    /// <item><description>High fatigue rate</description></item>
    /// <item><description>Low pressure resistance</description></item>
    /// </list>
    /// This preset is suitable for simulating novice players or casual players.
    /// </remarks>
    public EnhancedDartboardSimulatorBuilder WithBeginnerPlayer(string name = "Beginner")
    {
        _profile = PlayerProfile.Beginner(name);
        return this;
    }

    /// <summary>
    /// Configures the builder with a custom tremor (fatigue) model.
    /// </summary>
    /// <param name="model">The tremor model implementing fatigue accumulation logic.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// Tremor models simulate how player accuracy degrades over time due to fatigue.
    /// Common implementations include <see cref="LinearTremorModel"/> and
    /// <see cref="LogarithmicTremorModel"/>. Use <see cref="NoTremorModel"/> to disable
    /// fatigue simulation entirely.
    /// </remarks>
    /// <seealso cref="WithLinearTremor"/>
    /// <seealso cref="WithRealisticTremor"/>
    public EnhancedDartboardSimulatorBuilder WithTremorModel(ITremorModel model)
    {
        _tremorModel = model;
        return this;
    }

    /// <summary>
    /// Configures the builder with a linear tremor model for fatigue simulation.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// The linear tremor model increases fatigue at a constant rate throughout the session.
    /// Tremor accumulates proportionally to the number of throws, providing a simple and
    /// predictable fatigue pattern. This model is suitable for shorter sessions or when
    /// you want consistent fatigue accumulation.
    /// </remarks>
    /// <seealso cref="WithRealisticTremor"/>
    public EnhancedDartboardSimulatorBuilder WithLinearTremor()
    {
        _tremorModel = new LinearTremorModel();
        return this;
    }

    /// <summary>
    /// Configures the builder with a logarithmic tremor model for realistic fatigue simulation.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// The logarithmic tremor model simulates realistic fatigue accumulation where players
    /// experience rapid fatigue initially, which then plateaus over time. This mimics real-world
    /// performance degradation where players adapt to fatigue but never fully recover without rest.
    /// This model is recommended for realistic, longer-duration simulations.
    /// </remarks>
    /// <seealso cref="WithLinearTremor"/>
    public EnhancedDartboardSimulatorBuilder WithRealisticTremor()
    {
        _tremorModel = new LogarithmicTremorModel();
        return this;
    }

    /// <summary>
    /// Configures the builder with a custom pressure model for psychological effects.
    /// </summary>
    /// <param name="model">The pressure model implementing psychological pressure logic.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// Pressure models simulate how psychological factors affect player accuracy in high-stakes
    /// situations. Common implementations include <see cref="StandardPressureModel"/> and
    /// <see cref="CheckoutPsychologyModel"/>. Use <see cref="NoPressureModel"/> to disable
    /// pressure effects entirely.
    /// </remarks>
    /// <seealso cref="WithStandardPressure"/>
    /// <seealso cref="WithCheckoutPsychology"/>
    public EnhancedDartboardSimulatorBuilder WithPressureModel(IPressureModel model)
    {
        _pressureModel = model;
        return this;
    }

    /// <summary>
    /// Configures the builder with a standard pressure model.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// The standard pressure model applies moderate psychological pressure effects based on
    /// game context, such as remaining score and checkout situations. It provides a balanced
    /// simulation of how pressure affects player performance without extreme penalties.
    /// </remarks>
    /// <seealso cref="WithCheckoutPsychology"/>
    public EnhancedDartboardSimulatorBuilder WithStandardPressure()
    {
        _pressureModel = new StandardPressureModel();
        return this;
    }

    /// <summary>
    /// Configures the builder with a checkout psychology model for enhanced realism.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// The checkout psychology model simulates the heightened pressure players experience
    /// when attempting to finish a game (checkout). This model applies stronger accuracy
    /// penalties during high-pressure checkout situations, especially for difficult finishes
    /// like double attempts. Recommended for realistic 501 game simulations.
    /// </remarks>
    /// <seealso cref="WithStandardPressure"/>
    public EnhancedDartboardSimulatorBuilder WithCheckoutPsychology()
    {
        _pressureModel = new CheckoutPsychologyModel();
        return this;
    }

    /// <summary>
    /// Configures the builder with a custom momentum model for streak effects.
    /// </summary>
    /// <param name="model">The momentum model implementing hot/cold streak logic.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// Momentum models simulate how recent performance affects current throw accuracy.
    /// Players experiencing a "hot streak" gain temporary accuracy bonuses, while those
    /// on a "cold streak" suffer temporary penalties. Use <see cref="NoMomentumModel"/>
    /// to disable momentum effects entirely.
    /// </remarks>
    /// <seealso cref="WithStandardMomentum"/>
    public EnhancedDartboardSimulatorBuilder WithMomentumModel(IMomentumModel model)
    {
        _momentumModel = model;
        return this;
    }

    /// <summary>
    /// Configures the builder with a standard momentum model.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// The standard momentum model analyzes recent throw performance to determine if the
    /// player is on a hot or cold streak. Hot streaks provide temporary accuracy bonuses,
    /// while cold streaks apply temporary penalties. This creates realistic performance
    /// variations that mirror real-world player experiences.
    /// </remarks>
    public EnhancedDartboardSimulatorBuilder WithStandardMomentum()
    {
        _momentumModel = new StandardMomentumModel();
        return this;
    }

    /// <summary>
    /// Configures the builder with a custom grouping model for dart clustering effects.
    /// </summary>
    /// <param name="model">The grouping model implementing dart clustering logic.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// Grouping models simulate the tendency of darts to cluster together within the same
    /// visit (set of three throws). When enabled, subsequent darts in a visit are influenced
    /// by the positions of earlier darts, creating realistic clustering patterns. Use
    /// <see cref="NoGroupingModel"/> to disable grouping effects entirely.
    /// </remarks>
    /// <seealso cref="WithSimpleGrouping"/>
    public EnhancedDartboardSimulatorBuilder WithGroupingModel(IGroupingModel model)
    {
        _groupingModel = model;
        return this;
    }

    /// <summary>
    /// Configures the builder with a simple grouping model.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// The simple grouping model creates realistic dart clustering by making subsequent
    /// darts in a visit tend toward the positions of earlier darts. This simulates the
    /// natural phenomenon where players often throw similar trajectories within a single
    /// visit, creating visible dart clusters on the board.
    /// </remarks>
    public EnhancedDartboardSimulatorBuilder WithSimpleGrouping()
    {
        _groupingModel = new SimpleGroupingModel();
        return this;
    }

    /// <summary>
    /// Configures the builder with a custom target difficulty model.
    /// </summary>
    /// <param name="model">The target difficulty model implementing target-specific difficulty adjustments.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// Target difficulty models adjust throw accuracy based on the inherent difficulty of
    /// specific targets. For example, narrow segments like Triple 20 may be more challenging
    /// than larger targets like the outer singles. Use <see cref="NoTargetDifficultyModel"/>
    /// to treat all targets as equally difficult.
    /// </remarks>
    /// <seealso cref="WithStandardTargetDifficulty"/>
    public EnhancedDartboardSimulatorBuilder WithTargetDifficultyModel(ITargetDifficultyModel model)
    {
        _targetDifficultyModel = model;
        return this;
    }

    /// <summary>
    /// Configures the builder with a standard target difficulty model.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// The standard target difficulty model applies realistic difficulty adjustments based on
    /// target characteristics such as size, position, and proximity to adjacent high-value segments.
    /// This creates more realistic simulations where hitting small, valuable targets (like Triple 20)
    /// is appropriately challenging.
    /// </remarks>
    public EnhancedDartboardSimulatorBuilder WithStandardTargetDifficulty()
    {
        _targetDifficultyModel = new StandardTargetDifficultyModel();
        return this;
    }

    /// <summary>
    /// Configures the builder to truncate extreme deviations for more realistic throw distributions.
    /// </summary>
    /// <param name="maxDeviation">The maximum allowed deviation from the target. Defaults to 0.25 (normalized units).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// Truncation prevents extreme statistical outliers where darts would land impossibly far from
    /// the target. In real-world darts, even terrible throws stay within reasonable bounds due to
    /// physical limitations and board proximity.
    /// </para>
    /// <para>
    /// The maxDeviation parameter is specified in normalized units where 1.0 equals the radius of
    /// the double ring outer edge. A value of 0.25 means throws are limited to approximately one
    /// quarter of the board radius from the intended target.
    /// </para>
    /// <para>
    /// Recommended for realistic simulations to avoid unrealistic misses that would place darts
    /// off the board or in impossibly distant segments.
    /// </para>
    /// </remarks>
    public EnhancedDartboardSimulatorBuilder WithTruncation(double maxDeviation = 0.25)
    {
        _useTruncation = true;
        _maxDeviation = maxDeviation;
        return this;
    }

    /// <summary>
    /// Configures the builder to use a specific random number generator seed for reproducible simulations.
    /// </summary>
    /// <param name="seed">The seed value for the random number generator.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// Setting a seed ensures that simulations produce identical results across multiple runs,
    /// which is essential for debugging, testing, and reproducible analysis.
    /// </para>
    /// <para>
    /// When a seed is specified, the same sequence of random deviations will be generated for
    /// each session, making throw outcomes deterministic and repeatable. This is particularly
    /// useful when:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Writing unit tests that verify specific behaviors</description></item>
    /// <item><description>Debugging unexpected simulation outcomes</description></item>
    /// <item><description>Comparing different player profiles or behavioral models under identical conditions</description></item>
    /// <item><description>Demonstrating specific scenarios to stakeholders</description></item>
    /// </list>
    /// <para>
    /// If no seed is specified, the simulator uses a non-deterministic seed based on system time,
    /// producing different results on each run for realistic statistical variation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Reproducible simulation
    /// var session1 = new EnhancedDartboardSimulatorBuilder()
    ///     .WithProfessionalPlayer()
    ///     .WithSeed(42)
    ///     .BuildSession();
    ///
    /// var session2 = new EnhancedDartboardSimulatorBuilder()
    ///     .WithProfessionalPlayer()
    ///     .WithSeed(42)
    ///     .BuildSession();
    ///
    /// // Both sessions will produce identical throw sequences
    /// </code>
    /// </example>
    public EnhancedDartboardSimulatorBuilder WithSeed(int seed)
    {
        _seed = seed;
        return this;
    }

    /// <summary>
    /// Adds an event listener to receive notifications about completed throws.
    /// </summary>
    /// <param name="listener">The event listener to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// Event listeners implement the Observer pattern, receiving notifications whenever a throw
    /// is completed. This enables side effects like logging, real-time statistics, UI updates,
    /// or analytics without coupling these concerns to the core simulation logic.
    /// </para>
    /// <para>
    /// Multiple listeners can be registered, and they will all be notified in the order they
    /// were added. Each listener receives a <see cref="ThrowEvent"/> containing:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Throw result (segment hit, score, coordinates)</description></item>
    /// <item><description>Throw context (tremor, pressure, momentum, etc.)</description></item>
    /// <item><description>Player profile</description></item>
    /// <item><description>Session ID</description></item>
    /// <item><description>Timestamp</description></item>
    /// </list>
    /// <para>
    /// This method can be called multiple times to register multiple listeners.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var session = new EnhancedDartboardSimulatorBuilder()
    ///     .WithProfessionalPlayer()
    ///     .AddEventListener(new ConsoleLogger())
    ///     .AddEventListener(new CsvFileLogger("throws.csv"))
    ///     .AddEventListener(new StatisticsCollector())
    ///     .BuildSession();
    /// </code>
    /// </example>
    public EnhancedDartboardSimulatorBuilder AddEventListener(IThrowEventListener listener)
    {
        _eventListeners.Add(listener);
        return this;
    }

    /// <summary>
    /// Builds and returns a configured <see cref="PlayerSession"/> ready for simulation.
    /// </summary>
    /// <returns>
    /// A fully configured <see cref="PlayerSession"/> instance with all specified components
    /// and behavioral models wired together.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method constructs the final <see cref="PlayerSession"/> by assembling all configured
    /// components into a cohesive simulation system. The build process:
    /// </para>
    /// <list type="number">
    /// <item><description>Applies default values for any unconfigured components</description></item>
    /// <item><description>Creates the random number generator (seeded or non-deterministic)</description></item>
    /// <item><description>Constructs the deviation calculator chain using the Decorator pattern</description></item>
    /// <item><description>Wires together all behavioral models (tremor, pressure, momentum, grouping, difficulty)</description></item>
    /// <item><description>Creates the contextual simulator adapter</description></item>
    /// <item><description>Instantiates the PlayerSession with all components</description></item>
    /// </list>
    /// <para>
    /// <strong>Default Components:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description>Player Profile: Amateur</description></item>
    /// <item><description>Tremor Model: LinearTremorModel</description></item>
    /// <item><description>Pressure Model: NoPressureModel (disabled)</description></item>
    /// <item><description>Momentum Model: NoMomentumModel (disabled)</description></item>
    /// <item><description>Grouping Model: NoGroupingModel (disabled)</description></item>
    /// <item><description>Target Difficulty Model: NoTargetDifficultyModel (disabled)</description></item>
    /// <item><description>Random Seed: System time-based (non-deterministic)</description></item>
    /// <item><description>Truncation: Disabled</description></item>
    /// </list>
    /// <para>
    /// The builder instance can be reused to create multiple sessions with the same configuration.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Minimal configuration with defaults
    /// var simpleSession = new EnhancedDartboardSimulatorBuilder()
    ///     .BuildSession();
    ///
    /// // Full configuration
    /// var advancedSession = new EnhancedDartboardSimulatorBuilder()
    ///     .WithProfessionalPlayer("Alice")
    ///     .WithRealisticTremor()
    ///     .WithCheckoutPsychology()
    ///     .WithStandardMomentum()
    ///     .WithSimpleGrouping()
    ///     .WithStandardTargetDifficulty()
    ///     .WithTruncation(0.3)
    ///     .WithSeed(12345)
    ///     .AddEventListener(new ConsoleLogger())
    ///     .BuildSession();
    ///
    /// // Use the session
    /// var result = advancedSession.Throw(Target.Triple(20));
    /// </code>
    /// </example>
    public PlayerSession BuildSession()
    {
        // Defaults
        _profile ??= PlayerProfile.Amateur();
        _tremorModel ??= new LinearTremorModel();
        _pressureModel ??= new NoPressureModel();
        _momentumModel ??= new NoMomentumModel();
        _groupingModel ??= new NoGroupingModel();
        _targetDifficultyModel ??= new NoTargetDifficultyModel();

        var randomProvider = _seed.HasValue
            ? new DefaultRandomProvider(_seed.Value)
            : new DefaultRandomProvider();

        // Create base deviation calculator
        _baseDeviationCalculator ??= new GaussianDeviationCalculator(randomProvider);

        // Build decorator chain
        IContextualDeviationCalculator deviationCalculator =
            new SystematicBiasDeviationCalculator(_baseDeviationCalculator);

        deviationCalculator = new PressureModifiedDeviationCalculator(deviationCalculator);

        deviationCalculator = new MomentumModifiedDeviationCalculator(deviationCalculator);

        if (_useTruncation)
        {
            deviationCalculator = new TruncatedDeviationCalculator(
                deviationCalculator, _maxDeviation);
        }

        // Create contextual simulator
        var contextualSimulator = new ContextualSimulatorAdapter(
            deviationCalculator,
            _profile,
            _groupingModel,
            _targetDifficultyModel);

        // Create session
        return new PlayerSession(
            contextualSimulator,
            _profile,
            _tremorModel,
            _pressureModel,
            _momentumModel,
            _groupingModel,
            _targetDifficultyModel,
            _eventListeners);
    }
}
