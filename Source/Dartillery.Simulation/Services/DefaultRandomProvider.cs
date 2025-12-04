using Dartillery.Core.Abstractions;

namespace Dartillery.Simulation.Services;

/// <summary>
/// Default implementation of IRandomProvider using System.Random.
/// </summary>
internal sealed class DefaultRandomProvider : IRandomProvider
{
    private readonly Random _random;

    /// <summary>
    /// Initializes a new random provider with a time-dependent seed.
    /// </summary>
    public DefaultRandomProvider()
    {
        _random = new Random();
    }

    /// <summary>
    /// Initializes a new random provider with the specified seed.
    /// </summary>
    /// <param name="seed">The seed value for reproducible random sequences.</param>
    public DefaultRandomProvider(int seed)
    {
        _random = new Random(seed);
    }

    /// <inheritdoc />
    public double NextDouble() => _random.NextDouble();
}
