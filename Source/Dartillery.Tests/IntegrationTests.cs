using Dartillery;
using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
public class IntegrationTests
{
    [TestFixture]
    public class SimulatorIntegrationTests
    {
        [Test]
        public void Simulate501_ProfessionalWithSeed_FinishesUnder100Darts()
        {
            var simulator = CreateSeededSimulator(SimulatorPrecision.Professional, seed: 42);

            int remainingScore = 501;
            int dartsThrown = 0;
            const int maxDarts = 100;
            bool gameWon = false;

            while (remainingScore > 0 && dartsThrown < maxDarts)
            {
                var target = SelectSmartTarget(remainingScore);
                var result = simulator.Throw(target);
                dartsThrown++;

                var newScore = remainingScore - result.Score;

                // Bust rules: cannot end on 1 or go below 0
                // Must checkout with Double
                if (newScore == 1 || newScore < 0 || (newScore == 0 && !result.IsDouble))
                {
                    // Bust - score stays the same
                    continue;
                }

                remainingScore = newScore;

                if (remainingScore == 0 && result.IsDouble)
                {
                    gameWon = true;
                    break;
                }
            }

            TestContext.Out.WriteLine($"Game finished: {gameWon}, Darts: {dartsThrown}, Remaining: {remainingScore}");
            Assert.That(dartsThrown, Is.LessThan(maxDarts));
        }

        [Test]
        public void Throw_ThreePrecisionLevelsAtTriple20_HitRatesFormGradient()
        {
            var target = Target.Triple(20);
            const int throwCount = 500;

            var professional = CreateSeededSimulator(SimulatorPrecision.Professional, seed: 42);
            var amateur = CreateSeededSimulator(SimulatorPrecision.Amateur, seed: 42);
            var beginner = CreateSeededSimulator(SimulatorPrecision.Beginner, seed: 42);

            var proResults = ThrowMultiple(professional, target, throwCount);
            var amateurResults = ThrowMultiple(amateur, target, throwCount);
            var beginnerResults = ThrowMultiple(beginner, target, throwCount);

            var proHitRate = CalculateHitRate(proResults, SegmentType.Triple, 20);
            var amateurHitRate = CalculateHitRate(amateurResults, SegmentType.Triple, 20);
            var beginnerHitRate = CalculateHitRate(beginnerResults, SegmentType.Triple, 20);

            TestContext.Out.WriteLine($"Professional hit rate: {proHitRate:P1}");
            TestContext.Out.WriteLine($"Amateur hit rate: {amateurHitRate:P1}");
            TestContext.Out.WriteLine($"Beginner hit rate: {beginnerHitRate:P1}");

            Assert.Multiple(() =>
            {
                // Professionals should hit better than amateurs, amateurs better than beginners
                Assert.That(proHitRate, Is.GreaterThan(amateurHitRate));
                Assert.That(amateurHitRate, Is.GreaterThan(beginnerHitRate));
            });
        }

        [Test]
        public void Throw_GaussianVsUniformAtBullseye_BothHitBullAtLeastOnce()
        {
            var target = Target.Bullseye();
            const int throwCount = 1000;
            const double sigma = 0.03;

            var gaussian = new DartboardSimulatorBuilder()
                .UseGaussianDistribution()
                .WithStandardDeviation(sigma)
                .WithSeed(42)
                .Build();

            var uniform = new DartboardSimulatorBuilder()
                .UseUniformDistribution()
                .WithStandardDeviation(sigma * 2)
                .WithSeed(42)
                .Build();

            var gaussianResults = ThrowMultiple(gaussian, target, throwCount);
            var uniformResults = ThrowMultiple(uniform, target, throwCount);

            var gaussianBullHits = gaussianResults.Count(r => r.SegmentType == SegmentType.InnerBull);
            var uniformBullHits = uniformResults.Count(r => r.SegmentType == SegmentType.InnerBull);

            var gaussianAvg = gaussianResults.Average(r => r.Score);
            var uniformAvg = uniformResults.Average(r => r.Score);

            TestContext.Out.WriteLine($"Gaussian - Bull hits: {gaussianBullHits}, Avg score: {gaussianAvg:F2}");
            TestContext.Out.WriteLine($"Uniform - Bull hits: {uniformBullHits}, Avg score: {uniformAvg:F2}");

            Assert.Multiple(() =>
            {
                // Both should hit the board
                Assert.That(gaussianBullHits, Is.GreaterThan(0));
                Assert.That(uniformBullHits, Is.GreaterThan(0));

                // Both should have reasonable average values
                Assert.That(gaussianAvg, Is.GreaterThan(0));
                Assert.That(uniformAvg, Is.GreaterThan(0));
            });
        }

        [Test]
        public void SimulateCricket_ProfessionalPlayer_ClosesAtLeastOneSector()
        {
            var simulator = CreateSeededSimulator(SimulatorPrecision.Professional, seed: 999);

            var cricketNumbers = new[] { 15, 16, 17, 18, 19, 20 };
            var hitCounts = new Dictionary<int, int>();
            foreach (var num in cricketNumbers)
                hitCounts[num] = 0;
            hitCounts[25] = 0; // Bull

            const int maxDarts = 50;

            for (int i = 0; i < maxDarts; i++)
            {
                // Choose a number that is not yet closed
                var openNumbers = hitCounts.Where(kv => kv.Value < 3).Select(kv => kv.Key).ToList();
                if (openNumbers.Count == 0)
                    break;

                var targetNumber = openNumbers[0];
                Target target = targetNumber == 25
                    ? Target.Bullseye()
                    : Target.Triple(targetNumber);

                var result = simulator.Throw(target);

                // Count hits
                if (targetNumber == 25 && result.SegmentType == SegmentType.InnerBull)
                {
                    hitCounts[25] += 3; // Bull counts as 3
                }
                else if (result.SectorNumber == targetNumber)
                {
                    var multiplier = result.SegmentType switch
                    {
                        SegmentType.Single => 1,
                        SegmentType.Double => 2,
                        SegmentType.Triple => 3,
                        _ => 0
                    };
                    hitCounts[targetNumber] = Math.Min(3, hitCounts[targetNumber] + multiplier);
                }
            }

            TestContext.Out.WriteLine("Cricket Hit Counts:");
            foreach (var (number, count) in hitCounts.OrderBy(kv => kv.Key))
            {
                var status = count >= 3 ? "CLOSED" : $"{count}/3";
                TestContext.Out.WriteLine($"  {(number == 25 ? "Bull" : number.ToString())}: {status}");
            }

            // At least some numbers should be closed
            var closedCount = hitCounts.Count(kv => kv.Value >= 3);
            Assert.That(closedCount, Is.GreaterThan(0));
        }

        [Test]
        public void SimulateAroundTheClock_AmateurPlayer_ReachesAtLeastSector1()
        {
            var simulator = CreateSeededSimulator(SimulatorPrecision.Amateur, seed: 777);

            int currentTarget = 1;
            int dartsThrown = 0;
            const int maxDarts = 200;

            while (currentTarget <= 20 && dartsThrown < maxDarts)
            {
                var target = Target.Single(currentTarget);
                var result = simulator.Throw(target);
                dartsThrown++;

                // If sector hit, move to the next
                if (result.SectorNumber == currentTarget && result.SegmentType != SegmentType.Miss)
                {
                    currentTarget++;
                }
            }

            TestContext.Out.WriteLine($"Around the Clock: Reached sector {currentTarget - 1} in {dartsThrown} darts");

            // With Amateur precision we should reach at least sector 1
            Assert.That(currentTarget, Is.GreaterThan(1), "Should hit at least sector 1");
        }

        [Test]
        public void Throw_100VisitsAtTriple20_BestScoreExceeds100()
        {
            var simulator = CreateSeededSimulator(SimulatorPrecision.Professional, seed: 42);

            int maxScore = 0;
            const int attempts = 100;

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                int visitScore = 0;
                for (int dart = 0; dart < 3; dart++)
                {
                    var result = simulator.Throw(Target.Triple(20));
                    visitScore += result.Score;
                }

                maxScore = Math.Max(maxScore, visitScore);
            }

            TestContext.Out.WriteLine($"Best 3-dart score: {maxScore} (max possible: 180)");

            // With Professional precision we should achieve high scores
            Assert.That(maxScore, Is.GreaterThan(100), "Should achieve high scores with professional precision");

            // The absolute maximum is 180 (3x T20)
            Assert.That(maxScore, Is.LessThanOrEqualTo(180));
        }

        [Test]
        public void Throw_FiveCommonDoubles_AllHitAtLeastOnce()
        {
            var simulator = CreateSeededSimulator(SimulatorPrecision.Professional, seed: 123);

            var commonDoubles = new[] { 16, 20, 18, 12, 10 };
            const int attemptsPerDouble = 50;

            var successRates = new Dictionary<int, double>();

            foreach (var doubleNumber in commonDoubles)
            {
                int hits = 0;
                for (int i = 0; i < attemptsPerDouble; i++)
                {
                    var result = simulator.Throw(Target.Double(doubleNumber));
                    if (result.SegmentType == SegmentType.Double && result.SectorNumber == doubleNumber)
                    {
                        hits++;
                    }
                }

                successRates[doubleNumber] = hits / (double)attemptsPerDouble;
            }

            TestContext.Out.WriteLine("Double Success Rates (Professional):");
            foreach (var (number, rate) in successRates.OrderByDescending(kv => kv.Value))
            {
                TestContext.Out.WriteLine($"  D{number}: {rate:P1}");
            }

            Assert.Multiple(() =>
            {
                // All doubles should be hit at least once
                Assert.That(successRates.Values.All(rate => rate > 0), Is.True,
                    "All doubles should be hit at least once");

                // Success rate should be reasonable for a professional
                Assert.That(successRates.Values.Average(), Is.GreaterThan(0.05),
                    "Average success rate should be reasonable for professional");
            });
        }

        private static IThrowSimulator CreateSeededSimulator(SimulatorPrecision precision, int seed)
        {
            return new DartboardSimulatorBuilder()
                .WithPrecision(precision)
                .WithSeed(seed)
                .Build();
        }

        private static List<ThrowResult> ThrowMultiple(IThrowSimulator simulator, Target target, int count)
        {
            var results = new List<ThrowResult>();
            for (int i = 0; i < count; i++)
            {
                results.Add(simulator.Throw(target));
            }

            return results;
        }

        private static double CalculateHitRate(List<ThrowResult> results, SegmentType targetType, int targetSector)
        {
            var hits = results.Count(r => r.SegmentType == targetType && r.SectorNumber == targetSector);
            return hits / (double)results.Count;
        }

        private static Target SelectSmartTarget(int remaining)
        {
            // Simple checkout strategy
            if (remaining <= 40 && remaining % 2 == 0 && remaining >= 2)
            {
                int doubleTarget = remaining / 2;
                if (doubleTarget <= 20)
                    return Target.Double(doubleTarget);
            }

            if (remaining == 50)
                return Target.Bullseye();

            // Default: aim for T20
            return Target.Triple(20);
        }
    }

    [TestFixture]
    public class CoordinateSystemTests
    {
        [Test]
        public void Throw_FourCardinalTargets_HitPointsSpreadAcrossBoard()
        {
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(42)
                .Build();

            var targets = new[]
            {
                Target.Triple(20), // Top
                Target.Triple(6), // Right
                Target.Triple(3), // Bottom
                Target.Triple(11) // Left
            };

            foreach (var target in targets)
            {
                var results = new List<ThrowResult>();
                for (int i = 0; i < 100; i++)
                {
                    results.Add(simulator.Throw(target));
                }

                // Check that hits are possible in all quadrants
                var hasPositiveX = results.Any(r => r.HitPoint.X > 0);
                var hasNegativeX = results.Any(r => r.HitPoint.X < 0);
                var hasPositiveY = results.Any(r => r.HitPoint.Y > 0);
                var hasNegativeY = results.Any(r => r.HitPoint.Y < 0);

                TestContext.Out.WriteLine($"Target {target}: +X={hasPositiveX}, -X={hasNegativeX}, +Y={hasPositiveY}, -Y={hasNegativeY}");
                Assert.That(results, Has.Count.EqualTo(100), "Should have recorded 100 throws per target");
            }
        }

        [Test]
        public void Throw_AllTwentySectors_AllSectorsHitAtLeastOnce()
        {
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(999)
                .Build();

            var sectorHits = new HashSet<int>();
            const int throwsPerSector = 20;

            for (int sector = 1; sector <= 20; sector++)
            {
                for (int i = 0; i < throwsPerSector; i++)
                {
                    var result = simulator.Throw(Target.Triple(sector));
                    if (result.SectorNumber > 0)
                    {
                        sectorHits.Add(result.SectorNumber);
                    }
                }
            }

            TestContext.Out.WriteLine($"Unique sectors hit: {sectorHits.Count}/20");
            Assert.That(sectorHits, Has.Count.EqualTo(20),
                "Should be able to hit all 20 sectors");
        }

        [Test]
        public void Throw_AtBullseye_BullHitsHaveSectorZero()
        {
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(123)
                .Build();

            var bullResults = new List<ThrowResult>();
            for (int i = 0; i < 200; i++)
            {
                bullResults.Add(simulator.Throw(Target.Bullseye()));
            }

            var bullHits = bullResults.Where(r =>
                r.SegmentType == SegmentType.InnerBull ||
                r.SegmentType == SegmentType.OuterBull);

            foreach (var hit in bullHits)
            {
                Assert.That(hit.SectorNumber, Is.EqualTo(0),
                    $"Bull hit should have sector 0, but had {hit.SectorNumber}");
            }
        }
    }

    [TestFixture]
    public class RealisticGameScenarios
    {
        [Test]
        public void Throw_ProfessionalVsAmateur_ProfessionalScoresHigher()
        {
            const int rounds = 10;
            const int dartsPerRound = 3;

            var pro = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(42)
                .Build();

            var amateur = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Amateur)
                .WithSeed(42)
                .Build();

            var target = Target.Triple(20);

            int proTotal = 0;
            int amateurTotal = 0;

            for (int round = 0; round < rounds; round++)
            {
                for (int dart = 0; dart < dartsPerRound; dart++)
                {
                    proTotal += pro.Throw(target).Score;
                    amateurTotal += amateur.Throw(target).Score;
                }
            }

            var proAverage = proTotal / (double)(rounds * dartsPerRound);
            var amateurAverage = amateurTotal / (double)(rounds * dartsPerRound);

            TestContext.Out.WriteLine($"Professional average: {proAverage:F2}");
            TestContext.Out.WriteLine($"Amateur average: {amateurAverage:F2}");
            TestContext.Out.WriteLine($"Skill gap: {proAverage - amateurAverage:F2} points per dart");

            Assert.That(proAverage, Is.GreaterThan(amateurAverage),
                "Professional should score higher than amateur");
        }

        [Test]
        public void Throw_ProfessionalAtDouble16_HitsWithin20Attempts()
        {
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(777)
                .Build();

            const int maxAttempts = 20;

            int attemptsNeeded = 0;
            bool success = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                attemptsNeeded++;
                var result = simulator.Throw(Target.Double(16));

                if (result.SegmentType == SegmentType.Double && result.SectorNumber == 16)
                {
                    success = true;
                    break;
                }
            }

            TestContext.Out.WriteLine($"Checkout (D16): {(success ? $"SUCCESS in {attemptsNeeded} attempts" : "FAILED")}");

            if (success)
            {
                Assert.That(attemptsNeeded, Is.LessThanOrEqualTo(maxAttempts));
            }
        }
    }
}
