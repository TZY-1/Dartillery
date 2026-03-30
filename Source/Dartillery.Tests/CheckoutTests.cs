using Dartillery;
using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
[Category("Analysis")]
public class CheckoutTests
{
    [Test]
    public void SimulateCheckout_CommonRoutes_AllAttemptsRecorded()
    {
        var simulator = CreateSeededSimulator(SimulatorPrecision.Professional, seed: 42);

        var checkouts = new List<CheckoutRoute>
        {
            new(170, Target.Triple(20), Target.Triple(20), Target.Bullseye()),
            new(167, Target.Triple(20), Target.Triple(19), Target.Bullseye()),
            new(161, Target.Triple(20), Target.Triple(17), Target.Bullseye()),
            new(160, Target.Triple(20), Target.Triple(20), Target.Double(20)),
            new(141, Target.Triple(20), Target.Triple(19), Target.Double(12)),
            new(121, Target.Triple(20), Target.Triple(11), Target.Double(14)),
            new(120, Target.Triple(20), Target.Single(20), Target.Double(20)),
            new(110, Target.Triple(20), Target.Triple(10), Target.Double(20)),
            new(101, Target.Triple(20), Target.Triple(17), Target.Double(5)),
            new(100, Target.Triple(20), Target.Double(20), Target.Double(20)),
            new(90, Target.Triple(18), Target.Double(18), null),
            new(80, Target.Triple(20), Target.Double(10), null),
            new(50, Target.Bullseye(), null, null),
            new(50, Target.Single(18), Target.Double(16), null),
            new(40, Target.Double(20), null, null)
        };

        TestContext.Out.WriteLine("=== Checkout Analysis (Professional Player) ===");
        TestContext.Out.WriteLine($"{"Score",-6} {"Route",-30} {"Success %",-12} {"Avg Darts",-10}");
        TestContext.Out.WriteLine(new string('-', 62));

        var allStats = new List<CheckoutStatistics>();

        foreach (var checkout in checkouts.OrderByDescending(c => c.Score))
        {
            var stats = SimulateCheckout(simulator, checkout.Targets, 100);
            allStats.Add(stats);
            var routeStr = string.Join(" -> ", checkout.Targets.Where(t => t != null));
            TestContext.Out.WriteLine($"{checkout.Score,-6} {routeStr,-30} {stats.SuccessRate * 100,-11:F1}% {stats.AverageDarts,-10:F2}");
        }

        Assert.That(allStats.Any(s => s.Successes > 0), Is.True,
            "At least one checkout route should succeed for a professional player");
    }

    [Test]
    public void SimulateCheckout_FourStrategiesFor120_AllStatsComputed()
    {
        var simulator = CreateSeededSimulator(SimulatorPrecision.Professional, seed: 999);

        var strategies = new List<(string Name, Target?[] Route)>
        {
            ("T20-S20-D20", new[] { Target.Triple(20), Target.Single(20), Target.Double(20) }),
            ("T18-T18-D18", new[] { Target.Triple(18), Target.Triple(18), Target.Double(18) }),
            ("Bull-Bull-D10", new[] { Target.Bullseye(), Target.Bullseye(), Target.Double(10) }),
            ("T19-T19-D17", new[] { Target.Triple(19), Target.Triple(19), Target.Double(17) })
        };

        TestContext.Out.WriteLine("=== Strategy Comparison: 120 Checkout ===");
        TestContext.Out.WriteLine($"{"Strategy",-20} {"Success %",-12} {"Avg Darts",-10}");
        TestContext.Out.WriteLine(new string('-', 45));

        var allStats = new List<(string Name, CheckoutStatistics Stats)>();

        foreach (var (name, route) in strategies)
        {
            var stats = SimulateCheckout(simulator, route, 200);
            allStats.Add((name, stats));
            TestContext.Out.WriteLine($"{name,-20} {stats.SuccessRate * 100,-11:F1}% {stats.AverageDarts,-10:F2}");
        }

        Assert.That(allStats.All(s => s.Stats.Attempts > 0), Is.True,
            "All strategies should have recorded attempts");
    }

    [Test]
    public void SimulateCheckout_AllPrecisionLevelsOn141_AllStatsComputed()
    {
        var checkoutRoute = new[] { Target.Triple(20), Target.Triple(19), Target.Double(12) }; // 141

        var precisionLevels = new[]
        {
            (SimulatorPrecision.Professional, "Professional"),
            (SimulatorPrecision.Amateur, "Amateur"),
            (SimulatorPrecision.Beginner, "Beginner")
        };

        TestContext.Out.WriteLine("=== Precision Impact on 141 Checkout (T20-T19-D12) ===");
        TestContext.Out.WriteLine($"{"Level",-15} {"Success %",-12} {"Avg Darts",-10}");
        TestContext.Out.WriteLine(new string('-', 40));

        var results = new List<(string Name, CheckoutStatistics Stats)>();

        foreach (var (level, name) in precisionLevels)
        {
            var simulator = CreateSeededSimulator(level, seed: 42);

            var stats = SimulateCheckout(simulator, checkoutRoute, 200);
            results.Add((name, stats));
            TestContext.Out.WriteLine($"{name,-15} {stats.SuccessRate * 100,-11:F1}% {stats.AverageDarts,-10:F2}");
        }

        Assert.That(results[0].Stats.SuccessRate, Is.GreaterThanOrEqualTo(results[1].Stats.SuccessRate),
            "Professional should have at least as high a success rate as Amateur");
        Assert.That(results[1].Stats.SuccessRate, Is.GreaterThanOrEqualTo(results[2].Stats.SuccessRate),
            "Amateur should have at least as high a success rate as Beginner");
    }

    private static IThrowSimulator CreateSeededSimulator(SimulatorPrecision precision, int seed)
    {
        return new DartboardSimulatorBuilder()
            .WithPrecision(precision)
            .WithSeed(seed)
            .Build();
    }

    private static int TryCheckout(IThrowSimulator simulator, Target?[] route)
    {
        int dartsUsed = 0;

        foreach (var target in route)
        {
            if (target == null) continue;

            dartsUsed++;
            var result = simulator.Throw(target);

            if (!IsTargetHit(target, result))
            {
                return -1; // Checkout failed
            }
        }

        return dartsUsed;
    }

    private static bool IsTargetHit(Target target, ThrowResult result)
    {
        if (target.SegmentType is SegmentType.InnerBull or SegmentType.OuterBull)
        {
            return result.SegmentType == target.SegmentType;
        }

        return result.SegmentType == target.SegmentType &&
               result.SectorNumber == target.SectorNumber;
    }

    private static CheckoutStatistics SimulateCheckout(IThrowSimulator simulator, Target?[] route, int attempts)
    {
        int successes = 0;
        int totalDarts = 0;

        for (int i = 0; i < attempts; i++)
        {
            int dartsUsed = TryCheckout(simulator, route);
            if (dartsUsed > 0)
            {
                successes++;
                totalDarts += dartsUsed;
            }
        }

        return new CheckoutStatistics
        {
            Attempts = attempts,
            Successes = successes,
            SuccessRate = successes / (double)attempts,
            AverageDarts = successes > 0 ? totalDarts / (double)successes : 0
        };
    }

    private class CheckoutRoute
    {
        public CheckoutRoute(int score, params Target?[] targets)
        {
            Score = score;
            Targets = targets;
        }

        public int Score { get; }

        public Target?[] Targets { get; }
    }

    private class CheckoutStatistics
    {
        public int Attempts { get; set; }

        public int Successes { get; set; }

        public double SuccessRate { get; set; }

        public double AverageDarts { get; set; }
    }
}
