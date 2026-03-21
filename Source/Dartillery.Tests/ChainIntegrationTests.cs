using Dartillery;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

/// <summary>
/// Integration tests that verify the full EnhancedDartboardSimulatorBuilder decorator chain
/// (SystematicBiasDeviationCalculator -> PressureModifiedDeviationCalculator -> MomentumModifiedDeviationCalculator).
/// These tests act as a regression gate: if any rename or refactoring breaks the chain, they fail.
/// </summary>
[TestFixture]
[Category("Integration")]
public class ChainIntegrationTests
{
    /// <summary>
    /// Determinism test: two identically-configured sessions with the same fixed seed
    /// produce identical score sequences. This is the primary regression gate — any
    /// rename or refactoring that breaks the chain will cause this test to fail.
    /// </summary>
    [Test]
    public void BuildSession_TwoIdenticalSessionsWithFixedSeed_ProduceDeterministicScoreSequence()
    {
        const int throwCount = 20;
        var target = Target.Triple(20);

        var session1 = BuildFullChainSession(seed: 42);
        var session2 = BuildFullChainSession(seed: 42);

        var scores1 = new List<int>();
        var scores2 = new List<int>();

        for (int i = 0; i < throwCount; i++)
        {
            scores1.Add(session1.Throw(target).Score);
            scores2.Add(session2.Throw(target).Score);
        }

        TestContext.Out.WriteLine($"Session 1 scores: [{string.Join(", ", scores1)}]");
        TestContext.Out.WriteLine($"Session 2 scores: [{string.Join(", ", scores2)}]");

        for (int i = 0; i < throwCount; i++)
        {
            Assert.That(scores1[i], Is.EqualTo(scores2[i]),
                $"Score mismatch at throw {i + 1}: session1={scores1[i]}, session2={scores2[i]}");
        }
    }

    /// <summary>
    /// Decorator-active test: the full chain (with all behavioral models enabled) produces
    /// different score distributions than a bare DartboardSimulatorBuilder with the same seed.
    /// This proves the decorators are active and not no-ops.
    /// </summary>
    [Test]
    public void BuildSession_FullChainVsBareSimulator_ProducesDifferentScoreDistributions()
    {
        const int throwCount = 50;
        var target = Target.Triple(20);

        var fullChainSession = BuildFullChainSession(seed: 42);
        var bareSimulator = new DartboardSimulatorBuilder()
            .WithSeed(42)
            .Build();

        var fullChainScores = new List<int>();
        var bareScores = new List<int>();

        for (int i = 0; i < throwCount; i++)
        {
            fullChainScores.Add(fullChainSession.Throw(target).Score);
            bareScores.Add(bareSimulator.Throw(target).Score);
        }

        TestContext.Out.WriteLine($"Full chain scores: [{string.Join(", ", fullChainScores)}]");
        TestContext.Out.WriteLine($"Bare simulator scores: [{string.Join(", ", bareScores)}]");
        TestContext.Out.WriteLine($"Full chain avg: {fullChainScores.Average():F2}, Bare avg: {bareScores.Average():F2}");

        // The sequences must not be identical (decorators change the deviation calculation)
        bool sequencesAreIdentical = fullChainScores.SequenceEqual(bareScores);
        Assert.That(sequencesAreIdentical, Is.False,
            "Full decorator chain should produce different scores than bare simulator — decorators appear to be no-ops");
    }

    /// <summary>
    /// Output validity test: all results from the full chain have valid scores and coordinates.
    /// Verifies Score >= 0, SegmentType is a defined enum value, and HitPoint coordinates are finite.
    /// </summary>
    [Test]
    public void BuildSession_FullChain_AllResultsHaveValidScoresAndCoordinates()
    {
        var session = BuildFullChainSession(seed: 42);
        var targets = new[]
        {
            Target.Triple(20),
            Target.Double(16),
            Target.Bullseye(),
            Target.Single(5)
        };

        var results = new List<ThrowResult>();

        for (int i = 0; i < 30; i++)
        {
            var target = targets[i % targets.Length];
            results.Add(session.Throw(target));
        }

        var definedSegmentTypes = Enum.GetValues<SegmentType>().ToHashSet();

        foreach (var result in results)
        {
            TestContext.Out.WriteLine($"  {result}");

            Assert.That(result.Score, Is.GreaterThanOrEqualTo(0),
                $"Score must be >= 0, got {result.Score}");

            Assert.That(definedSegmentTypes.Contains(result.SegmentType), Is.True,
                $"SegmentType '{result.SegmentType}' is not a defined enum value");

            Assert.That(double.IsNaN(result.HitPoint.X), Is.False,
                $"HitPoint.X must not be NaN, got {result.HitPoint.X}");

            Assert.That(double.IsInfinity(result.HitPoint.X), Is.False,
                $"HitPoint.X must not be Infinity, got {result.HitPoint.X}");

            Assert.That(double.IsNaN(result.HitPoint.Y), Is.False,
                $"HitPoint.Y must not be NaN, got {result.HitPoint.Y}");

            Assert.That(double.IsInfinity(result.HitPoint.Y), Is.False,
                $"HitPoint.Y must not be Infinity, got {result.HitPoint.Y}");
        }

        TestContext.Out.WriteLine($"All {results.Count} results are valid.");
    }

    private static PlayerSession BuildFullChainSession(int seed = 42)
    {
        return new EnhancedDartboardSimulatorBuilder()
            .WithProfessionalPlayer("Test")
            .WithRealisticFatigue()
            .WithCheckoutPsychology()
            .WithStandardMomentum()
            .WithSeed(seed)
            .BuildSession();
    }
}
