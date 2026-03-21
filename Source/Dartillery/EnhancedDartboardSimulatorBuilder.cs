using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;
using Dartillery.Session;
using Dartillery.Simulation.Models.FatigueModels;
using Dartillery.Simulation.Models.GroupingModels;
using Dartillery.Simulation.Models.MomentumModels;
using Dartillery.Simulation.Models.PressureModels;
using Dartillery.Simulation.Models.TargetDifficultyModels;
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
/// <item><description><strong>Fatigue Models</strong>: Fatigue simulation (linear, logarithmic, or custom)</description></item>
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
/// // Simple professional player with linear fatigue
/// var session = new EnhancedDartboardSimulatorBuilder()
///     .WithProfessionalPlayer("Alice")
///     .WithLinearFatigue()
///     .BuildSession();
///
/// // Full-featured simulation with all behavioral models
/// var advancedSession = new EnhancedDartboardSimulatorBuilder()
///     .WithAmateurPlayer("Bob")
///     .WithRealisticFatigue()
///     .WithCheckoutPsychology()
///     .WithStandardMomentum()
///     .WithSimpleGrouping()
///     .WithStandardTargetDifficulty()
///     .WithTruncation()
///     .AddEventListener(new ConsoleLogger())
///     .BuildSession();
/// </code>
/// </example>
public sealed class EnhancedDartboardSimulatorBuilder
{
    private readonly List<IThrowEventListener> _eventListeners = new();
    private PlayerProfile? _profile;
    private IFatigueModel? _fatigueModel;
    private IPressureModel? _pressureModel;
    private IMomentumModel? _momentumModel;
    private IGroupingModel? _groupingModel;
    private ITargetDifficultyModel? _targetDifficultyModel;
    private SpreadMode _spreadMode = SpreadMode.Gaussian;
    private bool _useTruncation;
    private int? _seed;

    /// <summary>
    /// Configures the builder with a custom player profile.
    /// </summary>
    /// <param name="profile">The player profile defining skill characteristics and biases.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public EnhancedDartboardSimulatorBuilder WithPlayerProfile(PlayerProfile profile)
    {
        _profile = profile;
        return this;
    }

    /// <summary>
    /// Configures the builder with a professional-level player profile (high accuracy, low fatigue, strong pressure resistance).
    /// </summary>
    /// <param name="name">The player name. Defaults to "Pro".</param>
    /// <returns>The builder instance for method chaining.</returns>
    public EnhancedDartboardSimulatorBuilder WithProfessionalPlayer(string name = "Pro")
    {
        _profile = PlayerProfile.Professional(name);
        return this;
    }

    /// <summary>
    /// Configures the builder with an amateur-level player profile (moderate accuracy, noticeable biases, moderate pressure resistance).
    /// </summary>
    /// <param name="name">The player name. Defaults to "Amateur".</param>
    /// <returns>The builder instance for method chaining.</returns>
    public EnhancedDartboardSimulatorBuilder WithAmateurPlayer(string name = "Amateur")
    {
        _profile = PlayerProfile.Amateur(name);
        return this;
    }

    /// <summary>
    /// Configures the builder with a beginner-level player profile (low accuracy, significant biases, low pressure resistance).
    /// </summary>
    /// <param name="name">The player name. Defaults to "Beginner".</param>
    /// <returns>The builder instance for method chaining.</returns>
    public EnhancedDartboardSimulatorBuilder WithBeginnerPlayer(string name = "Beginner")
    {
        _profile = PlayerProfile.Beginner(name);
        return this;
    }

    /// <summary>
    /// Configures the builder with a custom fatigue model.
    /// </summary>
    /// <param name="model">The fatigue model implementing fatigue accumulation logic.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <seealso cref="WithLinearFatigue"/>
    /// <seealso cref="WithRealisticFatigue"/>
    public EnhancedDartboardSimulatorBuilder WithFatigueModel(IFatigueModel model)
    {
        _fatigueModel = model;
        return this;
    }

    /// <summary>
    /// Configures the builder with a linear fatigue model that accumulates fatigue at a constant rate.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    /// <seealso cref="WithRealisticFatigue"/>
    public EnhancedDartboardSimulatorBuilder WithLinearFatigue()
    {
        _fatigueModel = new LinearFatigueModel();
        return this;
    }

    /// <summary>
    /// Configures the builder with a logarithmic fatigue model that rises quickly then plateaus, matching real fatigue patterns.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    /// <seealso cref="WithLinearFatigue"/>
    public EnhancedDartboardSimulatorBuilder WithRealisticFatigue(double growthRate = 0.01)
    {
        _fatigueModel = new LogarithmicFatigueModel(growthRate);
        return this;
    }

    /// <summary>
    /// Disables fatigue simulation entirely. The player's accuracy will not degrade over the session.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public EnhancedDartboardSimulatorBuilder WithNoFatigue()
    {
        _fatigueModel = new NoFatigueModel();
        return this;
    }

    /// <summary>
    /// Configures the builder with a custom pressure model for psychological effects.
    /// </summary>
    /// <param name="model">The pressure model implementing psychological pressure logic.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <seealso cref="WithStandardPressure"/>
    /// <seealso cref="WithCheckoutPsychology"/>
    public EnhancedDartboardSimulatorBuilder WithPressureModel(IPressureModel model)
    {
        _pressureModel = model;
        return this;
    }

    /// <summary>
    /// Configures the builder with a standard pressure model that applies moderate accuracy penalties based on game context.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    /// <seealso cref="WithCheckoutPsychology"/>
    public EnhancedDartboardSimulatorBuilder WithStandardPressure()
    {
        _pressureModel = new StandardPressureModel();
        return this;
    }

    /// <summary>
    /// Configures the builder with a checkout psychology model that amplifies pressure during double/finish attempts.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    /// <seealso cref="WithStandardPressure"/>
    public EnhancedDartboardSimulatorBuilder WithCheckoutPsychology()
    {
        _pressureModel = new CheckoutPsychologyModel();
        return this;
    }

    /// <summary>
    /// Configures the builder with a custom momentum model for hot/cold streak effects.
    /// </summary>
    /// <param name="model">The momentum model implementing streak logic.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <seealso cref="WithStandardMomentum"/>
    public EnhancedDartboardSimulatorBuilder WithMomentumModel(IMomentumModel model)
    {
        _momentumModel = model;
        return this;
    }

    /// <summary>
    /// Configures the builder with a standard momentum model that boosts accuracy on hot streaks and penalizes cold ones.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public EnhancedDartboardSimulatorBuilder WithStandardMomentum(
        int windowSize = 6, double hotHandBonus = 0.05, double coldStreakPenalty = 0.1,
        double hotThreshold = 0.7, double coldThreshold = 0.5,
        double goodDeviationFactor = 1.0, double badDeviationFactor = 2.5)
    {
        double baseSigma = _profile?.BaseSkill ?? 0.05;
        _momentumModel = new StandardMomentumModel(
            baseSigma, windowSize, hotHandBonus, coldStreakPenalty,
            hotThreshold, coldThreshold, goodDeviationFactor, badDeviationFactor);
        return this;
    }

    /// <summary>
    /// Configures the builder with a custom grouping model for dart clustering effects.
    /// </summary>
    /// <param name="model">The grouping model implementing dart clustering logic.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <seealso cref="WithSimpleGrouping"/>
    public EnhancedDartboardSimulatorBuilder WithGroupingModel(IGroupingModel model)
    {
        _groupingModel = model;
        return this;
    }

    /// <summary>
    /// Configures the builder with a simple grouping model that biases subsequent darts in a visit toward earlier hit positions.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
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
    /// <seealso cref="WithStandardTargetDifficulty"/>
    public EnhancedDartboardSimulatorBuilder WithTargetDifficultyModel(ITargetDifficultyModel model)
    {
        _targetDifficultyModel = model;
        return this;
    }

    /// <summary>
    /// Configures the builder with a standard target difficulty model that scales deviation by segment size and position.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public EnhancedDartboardSimulatorBuilder WithStandardTargetDifficulty()
    {
        _targetDifficultyModel = new StandardTargetDifficultyModel();
        return this;
    }

    /// <summary>
    /// Sets the spread calculation mode (Gaussian or Uniform). Defaults to Gaussian.
    /// </summary>
    public EnhancedDartboardSimulatorBuilder WithSpreadMode(SpreadMode mode)
    {
        _spreadMode = mode;
        return this;
    }

    /// <summary>
    /// Enables deviation truncation to prevent statistical outliers from landing impossibly far from the target.
    /// The truncation bounds are automatically derived from the spread algorithm and player profile.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public EnhancedDartboardSimulatorBuilder WithTruncation()
    {
        _useTruncation = true;
        return this;
    }

    /// <summary>
    /// Sets a fixed seed for reproducible random number generation across multiple <see cref="BuildSession"/> calls.
    /// </summary>
    /// <param name="seed">The seed value for the random number generator.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public EnhancedDartboardSimulatorBuilder WithSeed(int seed)
    {
        _seed = seed;
        return this;
    }

    /// <summary>
    /// Registers an event listener to receive <see cref="ThrowEvent"/> notifications after each throw. Can be called multiple times.
    /// </summary>
    /// <param name="listener">The event listener to register.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public EnhancedDartboardSimulatorBuilder AddEventListener(IThrowEventListener listener)
    {
        _eventListeners.Add(listener);
        return this;
    }

    /// <summary>
    /// Builds and returns a configured <see cref="PlayerSession"/> ready for simulation.
    /// Unset components receive sensible defaults (Amateur profile, linear fatigue, no pressure/momentum/grouping/difficulty, no truncation).
    /// The builder can be reused — each call produces an independent session.
    /// </summary>
    /// <returns>A fully configured <see cref="PlayerSession"/> with all behavioral models wired together.</returns>
    public PlayerSession BuildSession()
    {
        // Capture all builder state into an immutable config — no ??= mutation of builder fields.
        // Each call produces a fresh config, so multiple BuildSession() calls are fully independent.
        var config = new SessionConfiguration
        {
            Profile = _profile ?? PlayerProfile.Amateur(),
            FatigueModel = _fatigueModel ?? new LinearFatigueModel(),
            PressureModel = _pressureModel ?? new NoPressureModel(),
            MomentumModel = _momentumModel ?? new NoMomentumModel(),
            GroupingModel = _groupingModel ?? new NoGroupingModel(),
            TargetDifficultyModel = _targetDifficultyModel ?? new NoTargetDifficultyModel(),
            SpreadMode = _spreadMode,
            UseTruncation = _useTruncation,
            Seed = _seed,
            EventListeners = _eventListeners.AsReadOnly()
        };

        var rng = config.Seed.HasValue
            ? new DefaultRandomProvider(config.Seed.Value)
            : new DefaultRandomProvider();

        IDeviationCalculator baseCalc = config.SpreadMode switch
        {
            SpreadMode.Uniform => new UniformDeviationCalculator(rng),
            _ => new GaussianDeviationCalculator(rng)
        };

        // Compute truncation bounds from the spread algorithm using the worst-case precision
        var maxPrecision = config.Profile.BaseSkill + config.Profile.MaxFatigue;
        var bounds = baseCalc.GetBounds(maxPrecision);

        var chain = new DeviationCalculatorChainBuilder(baseCalc)
            .WithTruncation(config.UseTruncation, bounds)
            .Build();

        return PlayerSessionFactory.Create(config, chain);
    }
}
