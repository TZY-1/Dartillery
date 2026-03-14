using Dartillery.Core.Abstractions;
using Dartillery.Simulation.Calculators;

namespace Dartillery.Session;

/// <summary>
/// Assembles the deviation calculator decorator chain in the correct order.
/// </summary>
internal sealed class DeviationCalculatorChainBuilder
{
    private readonly IDeviationCalculator _baseCalculator;
    private bool _useTruncation;
    private double _maxDeviation = 0.25;

    public DeviationCalculatorChainBuilder(IDeviationCalculator baseCalculator)
    {
        _baseCalculator = baseCalculator ?? throw new ArgumentNullException(nameof(baseCalculator));
    }

    /// <summary>
    /// Enables truncation with the specified maximum deviation.
    /// </summary>
    public DeviationCalculatorChainBuilder WithTruncation(bool use, double maxDeviation = 0.25)
    {
        _useTruncation = use;
        _maxDeviation = maxDeviation;
        return this;
    }

    /// <summary>
    /// Builds the decorator chain: Bias → Pressure → Momentum → (optional) Truncation.
    /// </summary>
    public IContextualDeviationCalculator Build()
    {
        IContextualDeviationCalculator chain =
            new SystematicBiasDeviationCalculator(_baseCalculator);

        chain = new PressureModifiedDeviationCalculator(chain);
        chain = new MomentumModifiedDeviationCalculator(chain);

        if (_useTruncation)
            chain = new TruncatedDeviationCalculator(chain, _maxDeviation);

        return chain;
    }
}
