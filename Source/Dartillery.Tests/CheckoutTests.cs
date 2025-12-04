using Dartillery;
using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
public class CheckoutTests
{
    [Test]
    public void AnalyzeCheckouts_CommonFinishes_ShowsSuccessRate()
    {
        // Arrange
        var simulator = new DartboardSimulatorBuilder()
            .WithPrecision(SimulatorPrecision.Professional)
            .WithSeed(42)
            .Build();

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

        Console.WriteLine("=== Checkout Analysis (Professional Player) ===");
        Console.WriteLine($"{"Score",-6} {"Route",-30} {"Success %",-12} {"Avg Darts",-10}");
        Console.WriteLine(new string('-', 62));

        foreach (var checkout in checkouts.OrderByDescending(c => c.Score))
        {
            var stats = SimulateCheckout(simulator, checkout.Targets, 100);
            var routeStr = string.Join(" -> ", checkout.Targets.Where(t => t != null));
            Console.WriteLine($"{checkout.Score,-6} {routeStr,-30} {stats.SuccessRate * 100,-11:F1}% {stats.AverageDarts,-10:F2}");
        }
    }

    [Test]
    public void CompareCheckoutStrategies_Score120_ShowsOptimalRoute()
    {
        // Arrange
        var simulator = new DartboardSimulatorBuilder()
            .WithPrecision(SimulatorPrecision.Professional)
            .WithSeed(999)
            .Build();

        var strategies = new List<(string Name, Target[] Route)>
        {
            ("T20-S20-D20", new[] { Target.Triple(20), Target.Single(20), Target.Double(20) }),
            ("T18-T18-D18", new[] { Target.Triple(18), Target.Triple(18), Target.Double(18) }),
            ("Bull-Bull-D10", new[] { Target.Bullseye(), Target.Bullseye(), Target.Double(10) }),
            ("T19-T19-D17", new[] { Target.Triple(19), Target.Triple(19), Target.Double(17) })
        };

        Console.WriteLine("=== Strategy Comparison: 120 Checkout ===");
        Console.WriteLine($"{"Strategy",-20} {"Success %",-12} {"Avg Darts",-10}");
        Console.WriteLine(new string('-', 45));

        foreach (var (name, route) in strategies)
        {
            var stats = SimulateCheckout(simulator, route, 200);
            Console.WriteLine($"{name,-20} {stats.SuccessRate * 100,-11:F1}% {stats.AverageDarts,-10:F2}");
        }
    }

    [Test]
    public void ComparePrecisionLevels_StandardCheckouts_ShowsDifference()
    {
        // Arrange
        var checkoutRoute = new[] { Target.Triple(20), Target.Triple(19), Target.Double(12) }; // 141

        var precisionLevels = new[]
        {
            (SimulatorPrecision.Professional, "Professional"),
            (SimulatorPrecision.Amateur, "Amateur"),
            (SimulatorPrecision.Beginner, "Beginner")
        };

        Console.WriteLine("=== Precision Impact on 141 Checkout (T20-T19-D12) ===");
        Console.WriteLine($"{"Level",-15} {"Success %",-12} {"Avg Darts",-10}");
        Console.WriteLine(new string('-', 40));

        foreach (var (level, name) in precisionLevels)
        {
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(level)
                .WithSeed(42)
                .Build();

            var stats = SimulateCheckout(simulator, checkoutRoute, 200);
            Console.WriteLine($"{name,-15} {stats.SuccessRate * 100,-11:F1}% {stats.AverageDarts,-10:F2}");
        }
    }

    private CheckoutStatistics SimulateCheckout(IThrowSimulator simulator, Target[] route, int attempts)
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

    private int TryCheckout(IThrowSimulator simulator, Target[] route)
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

    private bool IsTargetHit(Target target, ThrowResult result)
    {
        if (target.SegmentType is SegmentType.InnerBull or SegmentType.OuterBull)
        {
            return result.SegmentType == target.SegmentType;
        }

        return result.SegmentType == target.SegmentType &&
               result.SectorNumber == target.SectorNumber;
    }

    private class CheckoutRoute
    {
        public int Score { get; }
        public Target[] Targets { get; }

        public CheckoutRoute(int score, params Target[] targets)
        {
            Score = score;
            Targets = targets;
        }
    }

    private class CheckoutStatistics
    {
        public int Attempts { get; set; }
        public int Successes { get; set; }
        public double SuccessRate { get; set; }
        public double AverageDarts { get; set; }
    }
}
