using Dartillery.Core.Abstractions;

namespace Dartillery;

/// <summary>
/// Predefined simulator configurations for common use cases.
/// </summary>
public static class SimulatorPresets
{
    /// <summary>
    /// Minimal simulation: only bias, no fatigue or pressure.
    /// </summary>
    /// <param name="playerName">Player name.</param>
    /// <param name="seed">Optional random seed for reproducibility.</param>
    /// <returns>Configured player session.</returns>
    public static PlayerSession Simple(string playerName = "Player", int? seed = null)
    {
        var builder = new EnhancedDartboardSimulatorBuilder()
            .WithAmateurPlayer(playerName);

        if (seed.HasValue)
            builder.WithSeed(seed.Value);

        return builder.BuildSession();
    }

    /// <summary>
    /// Realistic simulation: all major features enabled.
    /// </summary>
    /// <param name="playerName">Player name.</param>
    /// <param name="seed">Optional random seed for reproducibility.</param>
    /// <returns>Configured player session with full realism.</returns>
    public static PlayerSession Realistic(string playerName = "Player", int? seed = null)
    {
        var builder = new EnhancedDartboardSimulatorBuilder()
            .WithAmateurPlayer(playerName)
            .WithRealisticTremor()
            .WithCheckoutPsychology()
            .WithStandardMomentum()
            .WithSimpleGrouping()
            .WithStandardTargetDifficulty();

        if (seed.HasValue)
            builder.WithSeed(seed.Value);

        return builder.BuildSession();
    }

    /// <summary>
    /// Professional player with high pressure resistance.
    /// </summary>
    /// <param name="playerName">Player name.</param>
    /// <param name="seed">Optional random seed for reproducibility.</param>
    /// <returns>Configured professional player session.</returns>
    public static PlayerSession Professional(string playerName = "Pro", int? seed = null)
    {
        var builder = new EnhancedDartboardSimulatorBuilder()
            .WithProfessionalPlayer(playerName)
            .WithRealisticTremor()
            .WithStandardPressure()
            .WithStandardMomentum();

        if (seed.HasValue)
            builder.WithSeed(seed.Value);

        return builder.BuildSession();
    }

    /// <summary>
    /// Bot training configuration: all features + event logging.
    /// </summary>
    /// <param name="playerName">Player/Bot name.</param>
    /// <param name="logger">Event listener for data logging.</param>
    /// <param name="seed">Optional random seed for reproducibility.</param>
    /// <returns>Configured session with event logging.</returns>
    public static PlayerSession BotTraining(
        string playerName,
        IThrowEventListener logger,
        int? seed = null)
    {
        var builder = new EnhancedDartboardSimulatorBuilder()
            .WithAmateurPlayer(playerName)
            .WithRealisticTremor()
            .WithCheckoutPsychology()
            .WithStandardMomentum()
            .WithSimpleGrouping()
            .WithStandardTargetDifficulty()
            .AddEventListener(logger);

        if (seed.HasValue)
            builder.WithSeed(seed.Value);

        return builder.BuildSession();
    }
}
