using Dartillery.Core.Abstractions;
using Dartillery.Simulation.Geometry;
using Dartillery.Simulation.Services;
using Dartillery.Simulation.Simulators;

namespace Dartillery;

/// <summary>
/// Fluent builder for creating and configuring dartboard simulators.
/// </summary>
public sealed class DartboardSimulatorBuilder
{
    private IDeviationCalculator? _deviationCalculator;
    private ISegmentResolver? _segmentResolver;
    private IAimPointCalculator? _aimPointCalculator;
    private IRandomProvider? _randomProvider;
    private double _standardDeviation = 0.03;
    private int? _seed;
    private DeviationDistribution _distributionType = DeviationDistribution.Gaussian;

    /// <summary>
    /// Configures the simulator to use Gaussian (normal) distribution for throw deviation.
    /// This is the recommended distribution for realistic dart throw simulation.
    /// </summary>
    public DartboardSimulatorBuilder UseGaussianDistribution()
    {
        _distributionType = DeviationDistribution.Gaussian;
        _deviationCalculator = null; // Will be created in Build()
        return this;
    }

    /// <summary>
    /// Configures the simulator to use uniform circular distribution for throw deviation.
    /// All throws land uniformly within a circle around the aim point.
    /// </summary>
    public DartboardSimulatorBuilder UseUniformDistribution()
    {
        _distributionType = DeviationDistribution.Uniform;
        _deviationCalculator = null; // Will be created in Build()
        return this;
    }

    /// <summary>
    /// Configures the simulator with a custom deviation calculator (advanced).
    /// </summary>
    internal DartboardSimulatorBuilder UseDeviationCalculator(IDeviationCalculator calculator)
    {
        _deviationCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        _distributionType = DeviationDistribution.Custom;
        return this;
    }

    /// <summary>
    /// Sets the standard deviation (precision) for throws.
    /// Lower values = more precise throws. Typical range: 0.02 (pro) to 0.08 (beginner).
    /// </summary>
    public DartboardSimulatorBuilder WithStandardDeviation(double sigma)
    {
        if (sigma <= 0)
            throw new ArgumentOutOfRangeException(nameof(sigma), "Standard deviation must be positive.");
        if (double.IsNaN(sigma))
            throw new ArgumentException("Standard deviation cannot be NaN.", nameof(sigma));
        if (double.IsInfinity(sigma))
            throw new ArgumentException("Standard deviation cannot be infinite.", nameof(sigma));

        _standardDeviation = sigma;
        return this;
    }

    /// <summary>
    /// Sets the precision using a predefined skill level.
    /// </summary>
    public DartboardSimulatorBuilder WithPrecision(SimulatorPrecision precision)
    {
        _standardDeviation = precision switch
        {
            SimulatorPrecision.Professional => 0.055,
            SimulatorPrecision.Amateur => 0.1,
            SimulatorPrecision.Beginner => 0.18,
            _ => 0.15
        };
        return this;
    }

    /// <summary>
    /// Sets a fixed seed for reproducible random number generation.
    /// Useful for testing and replay scenarios.
    /// </summary>
    public DartboardSimulatorBuilder WithSeed(int seed)
    {
        _seed = seed;
        return this;
    }

    /// <summary>
    /// Sets a custom random provider (advanced).
    /// </summary>
    internal DartboardSimulatorBuilder UseRandomProvider(IRandomProvider randomProvider)
    {
        _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
        return this;
    }

    /// <summary>
    /// Sets a custom segment resolver (advanced).
    /// </summary>
    internal DartboardSimulatorBuilder UseSegmentResolver(ISegmentResolver segmentResolver)
    {
        _segmentResolver = segmentResolver ?? throw new ArgumentNullException(nameof(segmentResolver));
        return this;
    }

    /// <summary>
    /// Sets a custom aim point calculator (advanced).
    /// </summary>
    internal DartboardSimulatorBuilder UseAimPointCalculator(IAimPointCalculator aimPointCalculator)
    {
        _aimPointCalculator = aimPointCalculator ?? throw new ArgumentNullException(nameof(aimPointCalculator));
        return this;
    }

    /// <summary>
    /// Builds the configured dartboard simulator.
    /// </summary>
    public IThrowSimulator Build()
    {
        // Create random provider if not provided
        _randomProvider ??= _seed.HasValue
            ? new DefaultRandomProvider(_seed.Value)
            : new DefaultRandomProvider();

        // Create deviation calculator if not provided
        if (_deviationCalculator == null)
        {
            _deviationCalculator = _distributionType switch
            {
                DeviationDistribution.Gaussian => new GaussianDeviationCalculator(_randomProvider),
                DeviationDistribution.Uniform => new UniformDeviationCalculator(_randomProvider),
                _ => new GaussianDeviationCalculator(_randomProvider)
            };
        }

        // Create default geometry services if not provided
        _segmentResolver ??= new SegmentResolver();
        _aimPointCalculator ??= new AimPointCalculator();

        return new DartboardSimulator(
            _deviationCalculator,
            _segmentResolver,
            _aimPointCalculator,
            _standardDeviation);
    }
}
