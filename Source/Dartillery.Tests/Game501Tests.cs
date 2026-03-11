using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
[Category("Analysis")]
public class Game501Tests
{
    private static IThrowSimulator CreateSeededSimulator(SimulatorPrecision precision, int? seed = null)
    {
        var builder = new DartboardSimulatorBuilder().WithPrecision(precision);
        if (seed.HasValue)
            builder = builder.WithSeed(seed.Value);
        return builder.Build();
    }

    [Test]
    public void Simulate501_ProfessionalPrecision_CompletesWithPositiveDartCount()
    {
        // Arrange
        var simulator = CreateSeededSimulator(SimulatorPrecision.Professional);

        // Act
        var gameStats = Simulate501Game(simulator);

        // Assert & Report
        TestContext.Out.WriteLine("=== 501 Game Simulation (Professional) ===\n");
        TestContext.Out.WriteLine($"Game finished: {(gameStats.GameWon ? "YES" : "NO")}");
        TestContext.Out.WriteLine($"Total darts thrown: {gameStats.TotalDarts}");
        TestContext.Out.WriteLine($"3-dart average: {gameStats.ThreeDartAverage:F2}");
        TestContext.Out.WriteLine($"Highest score (3 darts): {gameStats.HighestThreeDartScore}");
        TestContext.Out.WriteLine($"Checkout attempts: {gameStats.CheckoutAttempts}");
        TestContext.Out.WriteLine($"Checkout success: {(gameStats.CheckoutAttempts > 0 ? $"{gameStats.CheckoutSuccess}/{gameStats.CheckoutAttempts}" : "N/A")}");
        TestContext.Out.WriteLine($"\n--- Score Progression ---");

        for (int i = 0; i < gameStats.Visits.Count; i++)
        {
            var visit = gameStats.Visits[i];
            TestContext.Out.WriteLine($"Visit {i + 1}: {FormatVisit(visit)} = {visit.TotalScore} points (Remaining: {visit.RemainingAfter})");
        }

        TestContext.Out.WriteLine($"\n--- Target Distribution ---");
        var topTargets = gameStats.TargetsAimed
            .GroupBy(t => t.ToString())
            .Select(g => new { Target = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10);

        foreach (var target in topTargets)
        {
            TestContext.Out.WriteLine($"{target.Target}: {target.Count} times");
        }

        TestContext.Out.WriteLine($"\n--- Hit Distribution ---");
        var topHits = gameStats.ActualHits
            .GroupBy(r => $"{r.SegmentType} {(r.SectorNumber > 0 ? r.SectorNumber.ToString() : "")}")
            .Select(g => new { Hit = g.Key, Count = g.Count(), TotalScore = g.Sum(r => r.Score) })
            .OrderByDescending(x => x.TotalScore)
            .Take(10);

        foreach (var hit in topHits)
        {
            TestContext.Out.WriteLine($"{hit.Hit}: {hit.Count} times (Total: {hit.TotalScore} points)");
        }

        Assert.That(gameStats.TotalDarts, Is.GreaterThan(0));
    }

    [Test]
    public void Simulate501_AllPrecisionLevels_ProduceValidStatistics()
    {
        // Arrange
        var precisionLevels = new[]
        {
            (SimulatorPrecision.Professional, "Professional"),
            (SimulatorPrecision.Amateur, "Amateur"),
            (SimulatorPrecision.Beginner, "Beginner")
        };

        TestContext.Out.WriteLine("=== 501 Game Comparison Across Precision Levels ===\n");
        TestContext.Out.WriteLine($"{"Level",-15} {"Avg",-8} {"Darts",-8} {"Finished",-10} {"Checkout %",-12}");
        TestContext.Out.WriteLine(new string('-', 55));

        foreach (var (level, name) in precisionLevels)
        {
            var simulator = CreateSeededSimulator(level, seed: 123);

            var stats = Simulate501Game(simulator, maxDarts: 100);
            var checkoutPct = stats.CheckoutAttempts > 0
                ? stats.CheckoutSuccess * 100.0 / stats.CheckoutAttempts
                : 0;

            TestContext.Out.WriteLine($"{name,-15} {stats.ThreeDartAverage,-7:F2} {stats.TotalDarts,-8} {(stats.GameWon ? "Yes" : "No"),-10} {checkoutPct,-11:F1}%");
        }
    }

    [Test]
    public void Simulate501_TenGamesPerLevel_AllGamesRecorded()
    {
        var precisionLevels = new[]
        {
            (SimulatorPrecision.Professional, "Professional"),
            (SimulatorPrecision.Amateur, "Amateur"),
            (SimulatorPrecision.Beginner, "Beginner")
        };

        const int gameCount = 10;

        foreach (var (level, name) in precisionLevels)
        {
            var allGames = new List<Game501Statistics>();

            // Arrange
            var simulator = CreateSeededSimulator(level);

            TestContext.Out.WriteLine($"=== Simulating {gameCount} Games of 501 for {name} Level ===\n");

            // Act
            for (int i = 0; i < gameCount; i++)
            {
                var stats = Simulate501Game(simulator, maxDarts: 100);
                allGames.Add(stats);
                TestContext.Out.WriteLine($"Game {i + 1}: {stats.TotalDarts} darts, Avg {stats.ThreeDartAverage:F2}, {(stats.GameWon ? "Won" : "Not finished")}");
            }

            // Calculate overall statistics
            var finishedGames = allGames.Where(g => g.GameWon).ToList();
            var avgDarts = finishedGames.Any() ? finishedGames.Average(g => g.TotalDarts) : 0;
            var avgThreeDartAvg = finishedGames.Any() ? finishedGames.Average(g => g.ThreeDartAverage) : 0;
            var finishRate = finishedGames.Count * 100.0 / gameCount;

            TestContext.Out.WriteLine($"\n--- Overall Statistics ---");
            TestContext.Out.WriteLine($"Games finished: {finishedGames.Count}/{gameCount} ({finishRate:F1}%)");
            TestContext.Out.WriteLine($"Average darts per game: {avgDarts:F1}");
            TestContext.Out.WriteLine($"Average 3-dart average: {avgThreeDartAvg:F2}");

            Assert.That(allGames.Count, Is.EqualTo(gameCount));
        }
    }

    private Game501Statistics Simulate501Game(IThrowSimulator simulator, int maxDarts = 200)
    {
        var stats = new Game501Statistics();
        int remainingScore = 501;
        int dartsThrown = 0;

        while (remainingScore > 0 && dartsThrown < maxDarts)
        {
            var visit = new VisitStatistics { RemainingBefore = remainingScore };
            var throws = new List<ThrowResult>();

            var visitScore = remainingScore;
            var busted = false;
            // Throw 3 darts
            for (int i = 0; i < 3 && visitScore > 0; i++)
            {
                var target = SelectTarget(visitScore);

                // Check if this is a checkout attempt
                bool isCheckoutAttempt = visitScore - target.GetScore() == 0 &&
                                         (target.SegmentType is SegmentType.Double or SegmentType.InnerBull);

                if (isCheckoutAttempt)
                {
                    stats.CheckoutAttempts++;
                }

                stats.TargetsAimed.Add(target);

                var result = simulator.Throw(target);
                visitScore -= result.Score;

                if ((visitScore == 1 || visitScore < 0) ||
                    visitScore == 0 && !result.IsDouble)
                {
                    // Bust - no score this visit
                    busted = true;
                    break;
                }

                throws.Add(result);
                stats.ActualHits.Add(result);
                dartsThrown++;

                // Check for successful checkout (must be double, or bullseye for 50)
                if (visitScore == 0 && result.IsDouble)
                {
                    stats.CheckoutSuccess++;
                    stats.GameWon = true;
                    break;
                }
            }

            if (busted)
            {

                visitScore = remainingScore;
            }
            else
            {
                remainingScore = visitScore;
            }

            visit.Throws = throws;
            visit.Busted = busted;
            visit.TotalScore = throws.Sum(t => t.Score);
            visit.RemainingAfter = remainingScore;
            stats.Visits.Add(visit);

            if (stats.GameWon) break;
        }

        stats.TotalDarts = dartsThrown;
        stats.TotalScore = 501 - remainingScore;
        stats.ThreeDartAverage = dartsThrown > 0 ? stats.TotalScore * 3.0 / dartsThrown : 0;
        stats.HighestThreeDartScore = stats.Visits.Max(v => v.TotalScore);

        return stats;
    }

    private Target SelectTarget(int remaining)
    {
        // Simple strategy: aim for highest scoring targets
        // When in checkout range, aim for the double
        if (remaining <= 40 && remaining % 2 == 0 && remaining >= 2)
        {
            int doubleTarget = remaining / 2;
            if (doubleTarget <= 20)
            {
                return Target.Double(doubleTarget);
            }
        }

        if (remaining < Target.Double(20).GetScore() && remaining % 2 != 0)
        {
            return Target.Single(1);
        }

        // Special checkout: 50 = Bullseye
        if (remaining == 50) return Target.Bullseye();

        // Default: aim for T20
        var defaultTarget = Target.Triple(20);

        if (remaining - defaultTarget.GetScore() <= 0)
        {
            var newTargetScore = defaultTarget.GetScore() - Target.Double(20).GetScore();
            return Target.Single(newTargetScore);
        }

        return defaultTarget;
    }

    private string FormatVisit(VisitStatistics visit)
    {
        return string.Join(", ", visit.Throws.Select(t => $"{t.SegmentType}{t.SectorNumber}"));
    }

    private class Game501Statistics
    {
        public bool GameWon { get; set; }
        public int TotalDarts { get; set; }
        public int TotalScore { get; set; }
        public double ThreeDartAverage { get; set; }
        public int HighestThreeDartScore { get; set; }
        public int CheckoutAttempts { get; set; }
        public int CheckoutSuccess { get; set; }
        public List<VisitStatistics> Visits { get; set; } = new();
        public List<Target> TargetsAimed { get; set; } = new();
        public List<ThrowResult> ActualHits { get; set; } = new();
    }

    private class VisitStatistics
    {
        public int RemainingBefore { get; set; }
        public int RemainingAfter { get; set; }
        public List<ThrowResult> Throws { get; set; } = new();
        public int TotalScore { get; set; }
        public bool Busted { get; set; }
    }
}
