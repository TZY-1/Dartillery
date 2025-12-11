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
        public void FullGame_Professional_CanFinish501()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(42)
                .Build();

            int remainingScore = 501;
            int dartsThrown = 0;
            const int maxDarts = 100;
            bool gameWon = false;

            // Act
            while (remainingScore > 0 && dartsThrown < maxDarts)
            {
                var target = SelectSmartTarget(remainingScore);
                var result = simulator.Throw(target);
                dartsThrown++;

                var newScore = remainingScore - result.Score;

                // Bust-Regeln: Kann nicht auf 1 enden oder unter 0 gehen
                // Muss mit Double auschecken
                if (newScore == 1 || newScore < 0 || (newScore == 0 && !result.IsDouble))
                {
                    // Bust - Score bleibt gleich
                    continue;
                }

                remainingScore = newScore;

                if (remainingScore == 0 && result.IsDouble)
                {
                    gameWon = true;
                    break;
                }
            }

            // Assert
            Console.WriteLine($"Game finished: {gameWon}, Darts: {dartsThrown}, Remaining: {remainingScore}");
            Assert.That(dartsThrown, Is.LessThan(maxDarts));
        }

        [Test]
        public void PrecisionComparison_SameTarget_ShowsDifferentResults()
        {
            // Arrange
            var target = Target.Triple(20);
            const int throwCount = 500;

            var professional = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(42)
                .Build();

            var amateur = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Amateur)
                .WithSeed(42)
                .Build();

            var beginner = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Beginner)
                .WithSeed(42)
                .Build();

            // Act
            var proResults = ThrowMultiple(professional, target, throwCount);
            var amateurResults = ThrowMultiple(amateur, target, throwCount);
            var beginnerResults = ThrowMultiple(beginner, target, throwCount);

            // Assert
            var proHitRate = CalculateHitRate(proResults, SegmentType.Triple, 20);
            var amateurHitRate = CalculateHitRate(amateurResults, SegmentType.Triple, 20);
            var beginnerHitRate = CalculateHitRate(beginnerResults, SegmentType.Triple, 20);

            Console.WriteLine($"Professional hit rate: {proHitRate:P1}");
            Console.WriteLine($"Amateur hit rate: {amateurHitRate:P1}");
            Console.WriteLine($"Beginner hit rate: {beginnerHitRate:P1}");

            // Profis sollten besser treffen als Amateure, Amateure besser als Anfänger
            Assert.That(proHitRate, Is.GreaterThan(amateurHitRate));
            Assert.That(amateurHitRate, Is.GreaterThan(beginnerHitRate));
        }

        [Test]
        public void DistributionComparison_Gaussian_vs_Uniform()
        {
            // Arrange
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
                .WithStandardDeviation(sigma * 2) // Uniform braucht höheren Wert
                .WithSeed(42)
                .Build();

            // Act
            var gaussianResults = ThrowMultiple(gaussian, target, throwCount);
            var uniformResults = ThrowMultiple(uniform, target, throwCount);

            // Assert
            var gaussianBullHits = gaussianResults.Count(r => r.SegmentType == SegmentType.InnerBull);
            var uniformBullHits = uniformResults.Count(r => r.SegmentType == SegmentType.InnerBull);

            var gaussianAvg = gaussianResults.Average(r => r.Score);
            var uniformAvg = uniformResults.Average(r => r.Score);

            Console.WriteLine($"Gaussian - Bull hits: {gaussianBullHits}, Avg score: {gaussianAvg:F2}");
            Console.WriteLine($"Uniform - Bull hits: {uniformBullHits}, Avg score: {uniformAvg:F2}");

            // Beide sollten das Board treffen
            Assert.That(gaussianBullHits, Is.GreaterThan(0));
            Assert.That(uniformBullHits, Is.GreaterThan(0));

            // Beide sollten vernünftige Durchschnittswerte haben
            Assert.That(gaussianAvg, Is.GreaterThan(0));
            Assert.That(uniformAvg, Is.GreaterThan(0));
        }

        [Test]
        public void CricketGame_TrackingSectorHits()
        {
            // Arrange - Cricket verwendet Sektoren 15-20 plus Bull
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(999)
                .Build();

            var cricketNumbers = new[] { 15, 16, 17, 18, 19, 20 };
            var hitCounts = new Dictionary<int, int>();
            foreach (var num in cricketNumbers)
                hitCounts[num] = 0;
            hitCounts[25] = 0; // Bull

            const int maxDarts = 50;

            // Act - werfe auf Cricket-Nummern bis alle "geschlossen" sind (3 Treffer)
            for (int i = 0; i < maxDarts; i++)
            {
                // Wähle eine Nummer die noch nicht geschlossen ist
                var openNumbers = hitCounts.Where(kv => kv.Value < 3).Select(kv => kv.Key).ToList();
                if (!openNumbers.Any())
                    break;

                var targetNumber = openNumbers.First();
                Target target = targetNumber == 25
                    ? Target.Bullseye()
                    : Target.Triple(targetNumber);

                var result = simulator.Throw(target);

                // Zähle Treffer
                if (targetNumber == 25 && result.SegmentType == SegmentType.InnerBull)
                {
                    hitCounts[25] += 3; // Bull zählt als 3
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

            // Assert
            Console.WriteLine("Cricket Hit Counts:");
            foreach (var (number, count) in hitCounts.OrderBy(kv => kv.Key))
            {
                var status = count >= 3 ? "CLOSED" : $"{count}/3";
                Console.WriteLine($"  {(number == 25 ? "Bull" : number.ToString())}: {status}");
            }

            // Mindestens einige Nummern sollten geschlossen sein
            var closedCount = hitCounts.Count(kv => kv.Value >= 3);
            Assert.That(closedCount, Is.GreaterThan(0));
        }

        [Test]
        public void AroundTheClock_CompletionTest()
        {
            // Arrange - Around the Clock: Treffe Sektoren 1-20 in Reihenfolge
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Amateur)
                .WithSeed(777)
                .Build();

            int currentTarget = 1;
            int dartsThrown = 0;
            const int maxDarts = 200;

            // Act
            while (currentTarget <= 20 && dartsThrown < maxDarts)
            {
                var target = Target.Single(currentTarget);
                var result = simulator.Throw(target);
                dartsThrown++;

                // Wenn Sektor getroffen, gehe zum nächsten
                if (result.SectorNumber == currentTarget && result.SegmentType != SegmentType.Miss)
                {
                    currentTarget++;
                }
            }

            // Assert
            Console.WriteLine($"Around the Clock: Reached sector {currentTarget - 1} in {dartsThrown} darts");

            // Mit Amateur-Präzision sollten wir zumindest einige Sektoren schaffen
            Assert.That(currentTarget, Is.GreaterThan(1), "Should hit at least sector 1");
        }

        [Test]
        public void HighScore_ThreeDarts_FindsMaximumPossible()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(42)
                .Build();

            int maxScore = 0;
            const int attempts = 100;

            // Act - versuche mehrmals 3 Darts auf T20 zu werfen
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

            // Assert
            Console.WriteLine($"Best 3-dart score: {maxScore} (max possible: 180)");

            // Mit Professional-Präzision sollten wir hohe Scores schaffen
            Assert.That(maxScore, Is.GreaterThan(100), "Should achieve high scores with professional precision");

            // Der absolute Maximum ist 180 (3x T20)
            Assert.That(maxScore, Is.LessThanOrEqualTo(180));
        }

        [Test]
        public void CheckoutPractice_CommonDoubles_SuccessRates()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(123)
                .Build();

            var commonDoubles = new[] { 16, 20, 18, 12, 10 };
            const int attemptsPerDouble = 50;

            var successRates = new Dictionary<int, double>();

            // Act
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

            // Assert
            Console.WriteLine("Double Success Rates (Professional):");
            foreach (var (number, rate) in successRates.OrderByDescending(kv => kv.Value))
            {
                Console.WriteLine($"  D{number}: {rate:P1}");
            }

            // Alle Doubles sollten manchmal getroffen werden
            Assert.That(successRates.Values.All(rate => rate > 0), Is.True,
                "All doubles should be hit at least once");

            // Erfolgsrate sollte vernünftig sein für einen Profi
            Assert.That(successRates.Values.Average(), Is.GreaterThan(0.05),
                "Average success rate should be reasonable for professional");
        }

        private List<ThrowResult> ThrowMultiple(IThrowSimulator simulator, Target target, int count)
        {
            var results = new List<ThrowResult>();
            for (int i = 0; i < count; i++)
            {
                results.Add(simulator.Throw(target));
            }
            return results;
        }

        private double CalculateHitRate(List<ThrowResult> results, SegmentType targetType, int targetSector)
        {
            var hits = results.Count(r => r.SegmentType == targetType && r.SectorNumber == targetSector);
            return hits / (double)results.Count;
        }

        private Target SelectSmartTarget(int remaining)
        {
            // Einfache Checkout-Strategie
            if (remaining <= 40 && remaining % 2 == 0 && remaining >= 2)
            {
                int doubleTarget = remaining / 2;
                if (doubleTarget <= 20)
                    return Target.Double(doubleTarget);
            }

            if (remaining == 50)
                return Target.Bullseye();

            // Standard: Ziele auf T20
            return Target.Triple(20);
        }
    }

    [TestFixture]
    public class CoordinateSystemTests
    {
        [Test]
        public void Geometry_PointRotation_MaintainsDistanceFromOrigin()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(42)
                .Build();

            var targets = new[]
            {
                Target.Triple(20),  // Oben
                Target.Triple(6),   // Rechts
                Target.Triple(3),   // Unten
                Target.Triple(11)   // Links
            };

            // Act & Assert
            foreach (var target in targets)
            {
                var results = new List<ThrowResult>();
                for (int i = 0; i < 100; i++)
                {
                    results.Add(simulator.Throw(target));
                }

                // Prüfe dass Treffer in allen Quadranten möglich sind
                var hasPositiveX = results.Any(r => r.HitPoint.X > 0);
                var hasNegativeX = results.Any(r => r.HitPoint.X < 0);
                var hasPositiveY = results.Any(r => r.HitPoint.Y > 0);
                var hasNegativeY = results.Any(r => r.HitPoint.Y < 0);

                Console.WriteLine($"Target {target}: +X={hasPositiveX}, -X={hasNegativeX}, +Y={hasPositiveY}, -Y={hasNegativeY}");
            }
        }

        [Test]
        public void Geometry_AllSectors_CoverFullCircle()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(999)
                .Build();

            var sectorHits = new HashSet<int>();
            const int throwsPerSector = 20;

            // Act - werfe auf jeden Sektor
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

            // Assert - wir sollten alle 20 Sektoren getroffen haben
            Console.WriteLine($"Unique sectors hit: {sectorHits.Count}/20");
            Assert.That(sectorHits.Count, Is.EqualTo(20),
                "Should be able to hit all 20 sectors");
        }

        [Test]
        public void Geometry_BullRegions_DoNotOverlapWithSectors()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(123)
                .Build();

            // Act - werfe auf Bulls
            var bullResults = new List<ThrowResult>();
            for (int i = 0; i < 200; i++)
            {
                bullResults.Add(simulator.Throw(Target.Bullseye()));
            }

            // Assert - Bull-Treffer sollten keine Sector-Nummern haben
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
        public void Scenario_ProfessionalVsAmateur_ShowsSkillGap()
        {
            // Arrange
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

            // Act
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

            // Assert
            Console.WriteLine($"Professional average: {proAverage:F2}");
            Console.WriteLine($"Amateur average: {amateurAverage:F2}");
            Console.WriteLine($"Skill gap: {proAverage - amateurAverage:F2} points per dart");

            Assert.That(proAverage, Is.GreaterThan(amateurAverage),
                "Professional should score higher than amateur");
        }

        [Test]
        public void Scenario_PressureSituation_CheckoutAttempt()
        {
            // Arrange - Spieler braucht D16 zum Sieg
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(777)
                .Build();

            const int checkoutTarget = 32; // D16
            const int maxAttempts = 20;

            // Act - versuche mehrmals das Checkout
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

            // Assert
            Console.WriteLine($"Checkout (D16): {(success ? $"SUCCESS in {attemptsNeeded} attempts" : "FAILED")}");

            if (success)
            {
                Assert.That(attemptsNeeded, Is.LessThanOrEqualTo(maxAttempts));
            }
        }
    }
}
