using Dartillery.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dartillery.Tests;

[TestFixture]
public class DependencyInjectionTests
{
    [Test]
    public void AddDartillerySimulation_BuildServiceProvider_ResolvesAllPublicServices()
    {
        var services = new ServiceCollection();
        services.AddDartillerySimulation();

        using var provider = services.BuildServiceProvider();

        Assert.That(provider.GetRequiredService<IThrowSimulator>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<IDeviationCalculator>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<ISegmentResolver>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<IAimPointCalculator>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<ITargetResolver>(), Is.Not.Null);
    }

    [Test]
    public void AddDartilleryEnhanced_BuildServiceProvider_ResolvesEnhancedServices()
    {
        var services = new ServiceCollection();
        services.AddDartilleryEnhanced();

        using var provider = services.BuildServiceProvider();

        Assert.That(provider.GetRequiredService<IThrowSimulator>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<IDeviationCalculator>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<ISegmentResolver>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<IAimPointCalculator>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<ITargetResolver>(), Is.Not.Null);

        Assert.That(provider.GetRequiredService<ITremorModel>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<IPressureModel>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<IMomentumModel>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<IGroupingModel>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<ITargetDifficultyModel>(), Is.Not.Null);
    }
}
