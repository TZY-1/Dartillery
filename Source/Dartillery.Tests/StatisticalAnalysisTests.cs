using Dartillery;
using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
[Category("Analysis")]
public class StatisticalAnalysisTests
{
    [Test]
    public void Throw_MultiplePrecisionLevelsAtTriple20_ScoresPositiveForAllLevels()
    {
        var target = Target.Triple(20);
        const int throwCount = 1000;

        var precisionLevels = new[]
        {
            (Level: SimulatorPrecision.Professional, Name: "Professional"),
            (Level: SimulatorPrecision.Amateur, Name: "Amateur"),
            (Level: SimulatorPrecision.Beginner, Name: "Beginner")
        };

        TestContext.Out.WriteLine($"=== Precision Analysis: Aiming at {target} ===");
        TestContext.Out.WriteLine($"Throws per precision level: {throwCount}\n");

        foreach (var (level, name) in precisionLevels)
        {
            var simulator = CreateSeededSimulator(precision: level);
            var stats = AnalyzeThrows(simulator, target, throwCount);
            TestContext.Out.WriteLine($"--- {name} ---");
            PrintStatistics(stats, throwCount);

            Assert.That(stats.AverageScorePerDart, Is.GreaterThan(0));
        }
    }

    [Test]
    public void Throw_GaussianVsUniformAtBullseye_BothProducePositiveScores()
    {
        var target = Target.Bullseye();
        const int throwCount = 1000;
        const int seed = 123;

        TestContext.Out.WriteLine($"=== Distribution Comparison: Aiming at {target} ===");
        TestContext.Out.WriteLine($"Throws per distribution: {throwCount}\n");

        // Gaussian
        var gaussianSim = new DartboardSimulatorBuilder()
            .UseGaussianDistribution()
            .WithStandardDeviation(0.03)
            .WithSeed(seed)
            .Build();

        var gaussianStats = AnalyzeThrows(gaussianSim, target, throwCount);

        TestContext.Out.WriteLine("--- Gaussian Distribution (σ=0.03) ---");
        PrintStatistics(gaussianStats, throwCount);

        // Uniform
        var uniformSim = new DartboardSimulatorBuilder()
            .UseUniformDistribution()
            .WithStandardDeviation(0.06)
            .WithSeed(seed)
            .Build();

        var uniformStats = AnalyzeThrows(uniformSim, target, throwCount);

        TestContext.Out.WriteLine("--- Uniform Distribution (max radius=0.06) ---");
        PrintStatistics(uniformStats, throwCount);

        Assert.Multiple(() =>
        {
            Assert.That(gaussianStats.AverageScorePerDart, Is.GreaterThan(0));
            Assert.That(uniformStats.AverageScorePerDart, Is.GreaterThan(0));
        });
    }

    [Test]
    public void Throw_AllTargetTypesWithProfessional_AllScoresNonNegative()
    {
        var simulator = CreateSeededSimulator(precision: SimulatorPrecision.Professional, seed: 999);

        const int throwsPerTarget = 100;

        TestContext.Out.WriteLine("=== Target Analysis: Professional Player ===");
        TestContext.Out.WriteLine($"Throws per target: {throwsPerTarget}\n");

        var results = new List<(string Target, double HitRate, double AvgScore)>();

        // Test all doubles
        TestContext.Out.WriteLine("--- Doubles (Checkout Practice) ---");
        foreach (var sector in new[] { 20, 18, 16, 8, 4 })
        {
            var target = Target.Double(sector);
            var stats = AnalyzeThrows(simulator, target, throwsPerTarget);
            results.Add(($"D{sector}", stats.HitRate, stats.AverageScorePerDart));
            TestContext.Out.WriteLine($"D{sector}: Hit Rate {stats.HitRate:P1}, Avg Score {stats.AverageScorePerDart:F1}");
        }

        TestContext.Out.WriteLine("\n--- Triples (Scoring Practice) ---");
        foreach (var sector in new[] { 20, 19, 18, 17, 16 })
        {
            var target = Target.Triple(sector);
            var stats = AnalyzeThrows(simulator, target, throwsPerTarget);
            results.Add(($"T{sector}", stats.HitRate, stats.AverageScorePerDart));
            TestContext.Out.WriteLine($"T{sector}: Hit Rate {stats.HitRate:P1}, Avg Score {stats.AverageScorePerDart:F1}");
        }

        TestContext.Out.WriteLine("\n--- Bulls ---");
        var bullseye = AnalyzeThrows(simulator, Target.Bullseye(), throwsPerTarget);
        TestContext.Out.WriteLine($"Bullseye: Hit Rate {bullseye.HitRate:P1}, Avg Score {bullseye.AverageScorePerDart:F1}");

        var outerBull = AnalyzeThrows(simulator, Target.OuterBull(), throwsPerTarget);
        TestContext.Out.WriteLine($"Outer Bull: Hit Rate {outerBull.HitRate:P1}, Avg Score {outerBull.AverageScorePerDart:F1}");

        Assert.That(results, Is.Not.Empty);
        Assert.That(results.All(r => r.AvgScore >= 0), Is.True);
    }

    [Test]
    public void Throw_VaryingStandardDeviations_AllScoresNonNegative()
    {
        var target = Target.Triple(20);
        const int throwCount = 500;

        var standardDeviations = new[] { 0.01, 0.02, 0.03, 0.05, 0.08, 0.10 };

        TestContext.Out.WriteLine($"=== Standard Deviation Impact on {target} ===");
        TestContext.Out.WriteLine($"Throws per σ value: {throwCount}\n");
        TestContext.Out.WriteLine($"{"σ Value",-10} {"Hit Rate",-12} {"Avg Score",-12} {"Triple %",-12}");
        TestContext.Out.WriteLine(new string('-', 50));

        foreach (var sigma in standardDeviations)
        {
            var simulator = CreateSeededSimulator(standardDeviation: sigma, seed: 555);
            var stats = AnalyzeThrows(simulator, target, throwCount);
            var triplePercent = stats.TripleHits * 100.0 / throwCount;

            TestContext.Out.WriteLine($"{sigma,-10:F2} {stats.HitRate,-12:P1} {stats.AverageScorePerDart,-12:F2} {triplePercent,-12:F1}%");

            Assert.That(stats.AverageScorePerDart, Is.GreaterThanOrEqualTo(0));
        }
    }

    private static IThrowSimulator CreateSeededSimulator(
        SimulatorPrecision? precision = null,
        double? standardDeviation = null,
        int seed = 42)
    {
        var builder = new DartboardSimulatorBuilder().WithSeed(seed);

        if (precision.HasValue)
            builder = builder.WithPrecision(precision.Value);

        if (standardDeviation.HasValue)
            builder = builder.WithStandardDeviation(standardDeviation.Value);

        return builder.Build();
    }

    private static ThrowStatistics AnalyzeThrows(IThrowSimulator simulator, Target target, int throwCount)
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
                g => g.Count());

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

    private static void PrintStatistics(ThrowStatistics stats, int throwCount)
    {
        TestContext.Out.WriteLine($"Board Hit Rate: {stats.HitRate:P1} ({stats.SuccessfulThrows}/{throwCount})");

        TestContext.Out.WriteLine($"Average Score per Dart: {stats.AverageScorePerDart:F1}");

        TestContext.Out.WriteLine($"Three Dart Average: {stats.ThreeDartAverage:F1}");

        TestContext.Out.WriteLine($"Max Score: {stats.MaxScore}");

        TestContext.Out.WriteLine($"Triple Hits: {stats.TripleHits} ({stats.TripleHits * 100.0 / throwCount:F1}%)");

        TestContext.Out.WriteLine($"Misses: {stats.Misses} ({stats.Misses * 100.0 / throwCount:F1}%)");

        TestContext.Out.WriteLine("Top Scores:");

        foreach (var (score, count) in stats.ScoreDistribution.OrderByDescending(x => x.Value).Take(3))
        {
            TestContext.Out.WriteLine($"  {score} points: {count} times");
        }

        TestContext.Out.WriteLine("Score Distribution:");
        foreach (var (score, count) in stats.ScoreDistribution.OrderByDescending(x => x.Value))
        {
            TestContext.Out.WriteLine($"  {score} points: {count} times ({count * 100.0 / throwCount:F1}%)");
        }

        TestContext.Out.WriteLine("Field Distribution:");
        foreach (var ((segmenttype, score), count) in stats.FieldDistribution.OrderByDescending(x => x.Value))
        {
            TestContext.Out.WriteLine($"  {segmenttype} {score}: {count} ({count * 100.0 / throwCount:F1}%)");
        }

        TestContext.Out.WriteLine();
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

        public Dictionary<(SegmentType Type, int Sector), int> FieldDistribution { get; set; } = new();
    }
}
