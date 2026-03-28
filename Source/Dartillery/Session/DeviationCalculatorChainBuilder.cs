using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;
using Dartillery.Simulation.Calculators;

namespace Dartillery.Session;

/// <summary>
/// Assembles the deviation calculator decorator chain in the correct order.
/// </summary>
internal sealed class DeviationCalculatorChainBuilder
{
    private readonly IDeviationCalculator _baseCalculator;
    private bool _useTruncation;
    private ISpreadBounds? _truncationBounds;

    /// <summary>Initializes a new chain builder with the given base deviation calculator.</summary>
    public DeviationCalculatorChainBuilder(IDeviationCalculator baseCalculator)
    {
        ArgumentNullException.ThrowIfNull(baseCalculator);
        _baseCalculator = baseCalculator;
    }

    /// <summary>
    /// Enables truncation using the specified spread bounds.
    /// </summary>
    public DeviationCalculatorChainBuilder WithTruncation(bool use, ISpreadBounds? bounds = null)
    {
        _useTruncation = use;
        _truncationBounds = bounds;
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

        if (_useTruncation && _truncationBounds != null)
            chain = new TruncatedDeviationCalculator(chain, _truncationBounds);

        return chain;
    }
}
