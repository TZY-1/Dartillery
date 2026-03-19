using Dartillery;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
public class DartboardSimulatorTests
{
    [Test]
    public void Build_WithDefaultSettings_CreatesSimulator()
    {
        var simulator = new DartboardSimulatorBuilder()
            .Build();

        Assert.That(simulator, Is.Not.Null);
    }

    [Test]
    public void Throw_AtTriple20_ReturnsValidResult()
    {
        var simulator = new DartboardSimulatorBuilder()
            .WithSeed(42)
            .Build();
        var target = Target.Triple(20);

        var result = simulator.Throw(target);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Score, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.SectorNumber, Is.InRange(0, 20));
    }

    [Test]
    public void Throw_AtBullseye_ReturnsValidResult()
    {
        var simulator = new DartboardSimulatorBuilder()
            .WithSeed(123)
            .Build();
        var target = Target.Bullseye();

        var result = simulator.Throw(target);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Score, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void Throw_WithFixedSeed_ProducesConsistentResults()
    {
        var simulator1 = new DartboardSimulatorBuilder()
            .WithSeed(999)
            .Build();
        var simulator2 = new DartboardSimulatorBuilder()
            .WithSeed(999)
            .Build();
        var target = Target.Double(18);

        var result1 = simulator1.Throw(target);
        var result2 = simulator2.Throw(target);

        Assert.That(result1.Score, Is.EqualTo(result2.Score));
        Assert.That(result1.SegmentType, Is.EqualTo(result2.SegmentType));
        Assert.That(result1.SectorNumber, Is.EqualTo(result2.SectorNumber));
    }

    [Test]
    public void Throw_MultipleThrows_AllReturnValidScores()
    {
        var simulator = new DartboardSimulatorBuilder()
            .WithSeed(555)
            .Build();
        var target = Target.Triple(19);

        for (int i = 0; i < 100; i++)
        {
            var result = simulator.Throw(target);
            Assert.That(result.Score, Is.InRange(0, 60), $"Throw {i + 1} produced invalid score");
        }
    }

    [Test]
    public void Build_WithGaussianDistribution_CreatesSimulator()
    {
        var simulator = new DartboardSimulatorBuilder()
            .UseGaussianDistribution()
            .WithStandardDeviation(0.05)
            .Build();

        Assert.That(simulator, Is.Not.Null);
        var result = simulator.Throw(Target.Single(20));
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Build_WithUniformDistribution_CreatesSimulator()
    {
        var simulator = new DartboardSimulatorBuilder()
            .UseUniformDistribution()
            .WithStandardDeviation(0.06)
            .Build();

        Assert.That(simulator, Is.Not.Null);
        var result = simulator.Throw(Target.Single(15));
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Build_WithProfessionalPrecision_CreatesSimulator()
    {
        var simulator = new DartboardSimulatorBuilder()
            .WithPrecision(SimulatorPrecision.Professional)
            .WithSeed(777)
            .Build();
        var target = Target.Triple(20);

        var result = simulator.Throw(target);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Score, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void Build_WithAmateurPrecision_CreatesSimulator()
    {
        var simulator = new DartboardSimulatorBuilder()
            .WithPrecision(SimulatorPrecision.Amateur)
            .WithSeed(888)
            .Build();

        Assert.That(simulator, Is.Not.Null);
        var result = simulator.Throw(Target.Double(16));
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Build_WithBeginnerPrecision_CreatesSimulator()
    {
        var simulator = new DartboardSimulatorBuilder()
            .WithPrecision(SimulatorPrecision.Beginner)
            .WithSeed(111)
            .Build();

        Assert.That(simulator, Is.Not.Null);
        var result = simulator.Throw(Target.Single(1));
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Throw_AtAllSectors_ReturnsValidResults()
    {
        var simulator = new DartboardSimulatorBuilder()
            .WithSeed(2024)
            .Build();

        for (int sector = 1; sector <= 20; sector++)
        {
            var result = simulator.Throw(Target.Single(sector));
            Assert.That(result, Is.Not.Null, $"Sector {sector} returned null");
            Assert.That(result.Score, Is.GreaterThanOrEqualTo(0), $"Sector {sector} has invalid score");
        }
    }

    [Test]
    public void Throw_AtAllSegmentTypes_ReturnsValidResults()
    {
        var simulator = new DartboardSimulatorBuilder()
            .WithSeed(2025)
            .Build();

        var single = simulator.Throw(Target.Single(20));
        Assert.That(single, Is.Not.Null);

        var doubleResult = simulator.Throw(Target.Double(20));
        Assert.That(doubleResult, Is.Not.Null);

        var triple = simulator.Throw(Target.Triple(20));
        Assert.That(triple, Is.Not.Null);

        var bullseye = simulator.Throw(Target.Bullseye());
        Assert.That(bullseye, Is.Not.Null);

        var outerBull = simulator.Throw(Target.OuterBull());
        Assert.That(outerBull, Is.Not.Null);
    }
}
