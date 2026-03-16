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
    /// <returns>The builder instance for method chaining.</returns>
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
    /// <returns>The builder instance for method chaining.</returns>
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
        ArgumentNullException.ThrowIfNull(calculator);
        _deviationCalculator = calculator;
        _distributionType = DeviationDistribution.Custom;
        return this;
    }

    /// <summary>
    /// Sets the standard deviation (sigma) for throw deviation. Lower values produce more precise throws.
    /// Typical range: 0.02 (professional) to 0.08 (beginner).
    /// </summary>
    /// <param name="sigma">Standard deviation in normalized board units. Must be positive and finite.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DartboardSimulatorBuilder WithStandardDeviation(double sigma)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sigma);
        if (double.IsNaN(sigma))
            throw new ArgumentException("Standard deviation cannot be NaN.", nameof(sigma));
        if (double.IsInfinity(sigma))
            throw new ArgumentException("Standard deviation cannot be infinite.", nameof(sigma));

        _standardDeviation = sigma;
        return this;
    }

    /// <summary>
    /// Sets the standard deviation from a named skill-level preset (Professional, Amateur, or Beginner).
    /// </summary>
    /// <param name="precision">The target skill tier.</param>
    /// <returns>The builder instance for method chaining.</returns>
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
    /// Sets a fixed seed for reproducible random number generation. Useful for testing and replay scenarios.
    /// </summary>
    /// <param name="seed">The RNG seed value.</param>
    /// <returns>The builder instance for method chaining.</returns>
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
        ArgumentNullException.ThrowIfNull(randomProvider);
        _randomProvider = randomProvider;
        return this;
    }

    /// <summary>
    /// Sets a custom segment resolver (advanced).
    /// </summary>
    internal DartboardSimulatorBuilder UseSegmentResolver(ISegmentResolver segmentResolver)
    {
        ArgumentNullException.ThrowIfNull(segmentResolver);
        _segmentResolver = segmentResolver;
        return this;
    }

    /// <summary>
    /// Sets a custom aim point calculator (advanced).
    /// </summary>
    internal DartboardSimulatorBuilder UseAimPointCalculator(IAimPointCalculator aimPointCalculator)
    {
        ArgumentNullException.ThrowIfNull(aimPointCalculator);
        _aimPointCalculator = aimPointCalculator;
        return this;
    }

    /// <summary>
    /// Builds and returns a configured <see cref="IThrowSimulator"/> using the current builder state.
    /// </summary>
    /// <returns>A ready-to-use <see cref="IThrowSimulator"/> instance.</returns>
    public IThrowSimulator Build()
    {
        _randomProvider ??= _seed.HasValue
            ? new DefaultRandomProvider(_seed.Value)
            : new DefaultRandomProvider();

        if (_deviationCalculator == null)
        {
            _deviationCalculator = _distributionType switch
            {
                DeviationDistribution.Gaussian => new GaussianDeviationCalculator(_randomProvider),
                DeviationDistribution.Uniform => new UniformDeviationCalculator(_randomProvider),
                _ => new GaussianDeviationCalculator(_randomProvider)
            };
        }

        _segmentResolver ??= new SegmentResolver();
        _aimPointCalculator ??= new AimPointCalculator();

        return new DartboardSimulator(
            _deviationCalculator,
            _segmentResolver,
            _aimPointCalculator,
            _standardDeviation);
    }
}
