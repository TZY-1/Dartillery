using Dartillery;
using Dartillery.Core.Abstractions;
using Dartillery.Simulation.Geometry;
using Dartillery.Simulation.Services;
using Dartillery.Simulation.Simulators;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering Dartillery services with dependency injection.
/// </summary>
public static class DartilleryServiceCollectionExtensions
{
    /// <summary>
    /// Registers Dartillery simulation services with the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDartillerySimulation(
        this IServiceCollection services,
        Action<DartilleryOptions>? configure = null)
    {
        var options = new DartilleryOptions();
        configure?.Invoke(options);

        // Register random provider (singleton for consistent seeding)
        if (options.Seed.HasValue)
        {
            services.AddSingleton<IRandomProvider>(
                new DefaultRandomProvider(options.Seed.Value));
        }
        else
        {
            services.AddSingleton<IRandomProvider, DefaultRandomProvider>();
        }

        // Register deviation calculator based on distribution type
        RegisterDeviationCalculator(services, options);

        // Register geometry services
        services.AddSingleton<ISegmentResolver, SegmentResolver>();
        services.AddSingleton<IAimPointCalculator, AimPointCalculator>();
        services.AddSingleton<ITargetResolver, TargetResolver>();

        // Register main simulator
        services.AddSingleton(sp =>
        {
            var builder = new DartboardSimulatorBuilder()
                .UseDeviationCalculator(sp.GetRequiredService<IDeviationCalculator>())
                .UseSegmentResolver(sp.GetRequiredService<ISegmentResolver>())
                .UseAimPointCalculator(sp.GetRequiredService<IAimPointCalculator>())
                .WithStandardDeviation(options.StandardDeviation);

            return builder.Build();
        });

        // Register as interface for DI consumers
        services.AddSingleton<IThrowSimulator>(sp =>
            sp.GetRequiredService<DartboardSimulator>());

        return services;
    }

    private static void RegisterDeviationCalculator(
        IServiceCollection services,
        DartilleryOptions options)
    {
        switch (options.DistributionType)
        {
            case DeviationDistribution.Gaussian:
                services.AddSingleton<IDeviationCalculator, GaussianDeviationCalculator>();
                break;

            case DeviationDistribution.Uniform:
                services.AddSingleton<IDeviationCalculator, UniformDeviationCalculator>();
                break;

            case DeviationDistribution.Custom:
                if (options.CustomDeviationCalculator == null)
                {
                    throw new InvalidOperationException(
                        "CustomDeviationCalculator must be set when using DeviationDistribution.Custom.");
                }
                services.AddSingleton(options.CustomDeviationCalculator);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(options.DistributionType),
                    options.DistributionType,
                    "Unknown distribution type.");
        }
    }
}
