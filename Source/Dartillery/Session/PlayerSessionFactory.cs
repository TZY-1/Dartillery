using Dartillery.Core.Abstractions;
using Dartillery.Simulation.Adapters;

namespace Dartillery.Session;

/// <summary>
/// Creates <see cref="PlayerSession"/> instances from a configuration and deviation calculator chain.
/// </summary>
internal static class PlayerSessionFactory
{
    /// <summary>
    /// Creates a new player session using the provided configuration and deviation calculator.
    /// </summary>
    public static PlayerSession Create(
        SessionConfiguration config,
        IContextualDeviationCalculator deviationCalculator)
    {
        var simulator = new ContextualSimulatorAdapter(
            deviationCalculator,
            config.Profile,
            config.GroupingModel,
            config.TargetDifficultyModel);

        return new PlayerSession(
            simulator,
            config.Profile,
            config.FatigueModel,
            config.PressureModel,
            config.MomentumModel,
            config.GroupingModel,
            config.TargetDifficultyModel,
            config.EventListeners);
    }
}
