using Dartillery;
using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
public class StatisticalAnalysisTests
{
    [Test]
    public void AnalyzePrecisionLevels_Triple20_ShowsStatistics()
    {
        var target = Target.Triple(20);
        const int throwCount = 1000;
        var seed = 42;

        var precisionLevels = new[]
        {
            (Level: SimulatorPrecision.Professional, Name: "Professional"),
            (Level: SimulatorPrecision.Amateur, Name: "Amateur"),
            (Level: SimulatorPrecision.Beginner, Name: "Beginner")
        };

        Console.WriteLine($"=== Precision Analysis: Aiming at {target} ===");
        Console.WriteLine($"Throws per precision level: {throwCount}\n");

        foreach (var (level, name) in precisionLevels)
        {
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(level)
                .WithSeed(seed)
                .Build();

            var stats = AnalyzeThrows(simulator, target, throwCount);
            Console.WriteLine($"--- {name} ---");
            PrintStatistics(stats, throwCount);

            Assert.That(stats.AverageScorePerDart, Is.GreaterThan(0));
        }
    }

    [Test]
    public void AnalyzeDistributions_Bullseye_ComparesGaussianVsUniform()
    {
        // Arrange
        var target = Target.Bullseye();
        const int throwCount = 1000;
        var seed = 123;

        Console.WriteLine($"=== Distribution Comparison: Aiming at {target} ===");
        Console.WriteLine($"Throws per distribution: {throwCount}\n");

        // Gaussian
        var gaussianSim = new DartboardSimulatorBuilder()
            .UseGaussianDistribution()
            .WithStandardDeviation(0.03)
            .WithSeed(seed)
            .Build();

        var gaussianStats = AnalyzeThrows(gaussianSim, target, throwCount);

        Console.WriteLine("--- Gaussian Distribution (σ=0.03) ---");
        PrintStatistics(gaussianStats, throwCount);

        // Uniform
        var uniformSim = new DartboardSimulatorBuilder()
            .UseUniformDistribution()
            .WithStandardDeviation(0.06)
            .WithSeed(seed)
            .Build();

        var uniformStats = AnalyzeThrows(uniformSim, target, throwCount);

        Console.WriteLine("--- Uniform Distribution (max radius=0.06) ---");
        PrintStatistics(uniformStats, throwCount);

        // Assert
        Assert.That(gaussianStats.AverageScorePerDart, Is.GreaterThan(0));
        Assert.That(uniformStats.AverageScorePerDart, Is.GreaterThan(0));
    }

    [Test]
    public void AnalyzeAllTargets_Professional_ShowsHeatmap()
    {
        // Arrange
        var simulator = new DartboardSimulatorBuilder()
            .WithPrecision(SimulatorPrecision.Professional)
            .WithSeed(999)
            .Build();

        const int throwsPerTarget = 100;

        Console.WriteLine("=== Target Analysis: Professional Player ===");
        Console.WriteLine($"Throws per target: {throwsPerTarget}\n");

        var results = new List<(string Target, double HitRate, double AvgScore)>();

        // Test all doubles
        Console.WriteLine("--- Doubles (Checkout Practice) ---");
        foreach (var sector in new[] { 20, 18, 16, 8, 4 })
        {
            var target = Target.Double(sector);
            var stats = AnalyzeThrows(simulator, target, throwsPerTarget);
            results.Add(($"D{sector}", stats.HitRate, stats.AverageScorePerDart));
            Console.WriteLine($"D{sector}: Hit Rate {stats.HitRate:P1}, Avg Score {stats.AverageScorePerDart:F1}");
        }

        Console.WriteLine("\n--- Triples (Scoring Practice) ---");
        foreach (var sector in new[] { 20, 19, 18, 17, 16 })
        {
            var target = Target.Triple(sector);
            var stats = AnalyzeThrows(simulator, target, throwsPerTarget);
            results.Add(($"T{sector}", stats.HitRate, stats.AverageScorePerDart));
            Console.WriteLine($"T{sector}: Hit Rate {stats.HitRate:P1}, Avg Score {stats.AverageScorePerDart:F1}");
        }

        Console.WriteLine("\n--- Bulls ---");
        var bullseye = AnalyzeThrows(simulator, Target.Bullseye(), throwsPerTarget);
        Console.WriteLine($"Bullseye: Hit Rate {bullseye.HitRate:P1}, Avg Score {bullseye.AverageScorePerDart:F1}");

        var outerBull = AnalyzeThrows(simulator, Target.OuterBull(), throwsPerTarget);
        Console.WriteLine($"Outer Bull: Hit Rate {outerBull.HitRate:P1}, Avg Score {outerBull.AverageScorePerDart:F1}");

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.All(r => r.AvgScore >= 0), Is.True);
    }

    [Test]
    public void ComparePrecisionImpact_ShowsRelationship()
    {
        // Arrange
        var target = Target.Triple(20);
        const int throwCount = 500;

        var standardDeviations = new[] { 0.01, 0.02, 0.03, 0.05, 0.08, 0.10 };

        Console.WriteLine($"=== Standard Deviation Impact on {target} ===");
        Console.WriteLine($"Throws per σ value: {throwCount}\n");
        Console.WriteLine($"{"σ Value",-10} {"Hit Rate",-12} {"Avg Score",-12} {"Triple %",-12}");
        Console.WriteLine(new string('-', 50));

        foreach (var sigma in standardDeviations)
        {
            var simulator = new DartboardSimulatorBuilder()
                .WithStandardDeviation(sigma)
                .WithSeed(555)
                .Build();

            var stats = AnalyzeThrows(simulator, target, throwCount);
            var triplePercent = stats.TripleHits * 100.0 / throwCount;

            Console.WriteLine($"{sigma,-10:F2} {stats.HitRate,-12:P1} {stats.AverageScorePerDart,-12:F2} {triplePercent,-12:F1}%");

            Assert.That(stats.AverageScorePerDart, Is.GreaterThanOrEqualTo(0));
        }
    }

    private ThrowStatistics AnalyzeThrows(IThrowSimulator simulator, Target target, int throwCount)
    {
        var results = new List<ThrowResult>();

        for (int i = 0; i < throwCount; i++)
        {
            results.Add(simulator.Throw(target));
        }

        var hits = results.Count(r => r.SegmentType != SegmentType.Miss);
        var scoreDistribution = results
            .GroupBy(r => r.Score)
            .ToDictionary(g => g.Key, g => g.Count());

        var fieldDistribution = results
            .Where(r => r.SegmentType != SegmentType.Miss)
            .GroupBy(r => new { r.SegmentType, r.SectorNumber })
            .ToDictionary(
                g => (g.Key.SegmentType, g.Key.SectorNumber),
                g => g.Count()
            );

        return new ThrowStatistics
        {
            TotalThrows = throwCount,
            SuccessfulThrows = hits,
            Misses = results.Count(r => r.SegmentType == SegmentType.Miss),
            HitRate = hits / (double)throwCount,
            AverageScorePerDart = results.Average(r => r.Score),
            MaxScore = results.Max(r => r.Score),
            TripleHits = results.Count(r => r.SegmentType == SegmentType.Triple),
            DoubleHits = results.Count(r => r.SegmentType == SegmentType.Double),
            ScoreDistribution = scoreDistribution,
            FieldDistribution = fieldDistribution
        };
    }

    private void PrintStatistics(ThrowStatistics stats, int throwCount)
    {
        Console.WriteLine($"Board Hit Rate: {stats.HitRate:P1} ({stats.SuccessfulThrows}/{throwCount})");

        Console.WriteLine($"Average Score per Dart: {stats.AverageScorePerDart:F1}");

        Console.WriteLine($"Three Dart Average: {stats.ThreeDartAverage:F1}");

        Console.WriteLine($"Max Score: {stats.MaxScore}");

        Console.WriteLine($"Triple Hits: {stats.TripleHits} ({stats.TripleHits * 100.0 / throwCount:F1}%)");

        Console.WriteLine($"Misses: {stats.Misses} ({stats.Misses * 100.0 / throwCount:F1}%)");

        Console.WriteLine($"Top Scores:");

        foreach (var (score, count) in stats.ScoreDistribution.OrderByDescending(x => x.Value).Take(3))
        {
            Console.WriteLine($"  {score} points: {count} times");
        }

        Console.WriteLine($"Score Distribution:");
        foreach (var (score, count) in stats.ScoreDistribution.OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"  {score} points: {count} times ({count * 100.0 / throwCount:F1}%)");
        }

        Console.WriteLine($"Field Distribution:");
        foreach (var ((segmenttype, score), count) in stats.FieldDistribution.OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"  {segmenttype} {score}: {count} ({count * 100.0 / throwCount:F1}%)");
        }

        Console.WriteLine();
    }

    private class ThrowStatistics
    {
        public int TotalThrows { get; set; }
        public int SuccessfulThrows { get; set; }
        public int Misses { get; set; }
        public double HitRate { get; set; }
        public double AverageScorePerDart { get; set; }
        public double ThreeDartAverage => AverageScorePerDart * 3;
        public int MaxScore { get; set; }
        public int TripleHits { get; set; }
        public int DoubleHits { get; set; }
        public Dictionary<int, int> ScoreDistribution { get; set; } = new();
        public Dictionary<(SegmentType, int), int> FieldDistribution { get; set; } = new();
    }
}
