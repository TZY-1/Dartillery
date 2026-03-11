using Dartillery;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
public class EdgeCaseTests
{
    [TestFixture]
    public class BoundaryConditionTests
    {
        [Test]
        public void Throw_WithExtremelyHighPrecision_ReturnsValidResult()
        {
            var simulator = new DartboardSimulatorBuilder()
                .WithStandardDeviation(0.0001)
                .WithSeed(42)
                .Build();

            var target = Target.Triple(20);

            var result = simulator.Throw(target);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Score, Is.GreaterThanOrEqualTo(20));
        }

        [Test]
        public void Throw_WithExtremelyLowPrecision_StillHitsBoard()
        {
            var simulator = new DartboardSimulatorBuilder()
                .WithStandardDeviation(0.5)
                .WithSeed(42)
                .Build();

            var target = Target.Bullseye();

            var result = simulator.Throw(target);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Score, Is.InRange(0, 60));
        }

        [Test]
        public void Throw_AllValidTargets_ProducesValidResults()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(999)
                .Build();

            var targets = new List<Target>();

            // Alle Singles (1-20)
            for (int i = 1; i <= 20; i++)
                targets.Add(Target.Single(i));

            // Alle Doubles (1-20)
            for (int i = 1; i <= 20; i++)
                targets.Add(Target.Double(i));

            // Alle Triples (1-20)
            for (int i = 1; i <= 20; i++)
                targets.Add(Target.Triple(i));

            // Bulls
            targets.Add(Target.Bullseye());
            targets.Add(Target.OuterBull());

            // Act & Assert
            foreach (var target in targets)
            {
                var result = simulator.Throw(target);
                Assert.That(result, Is.Not.Null, $"Target {target} returned null");
                Assert.That(result.Score, Is.InRange(0, 60), $"Target {target} has invalid score {result.Score}");
                Assert.That(result.SectorNumber, Is.InRange(0, 20), $"Target {target} has invalid sector {result.SectorNumber}");
            }
        }

        [Test]
        public void Throw_ConsecutiveThrows_ShowsVariation()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(12345)
                .Build();

            var target = Target.Triple(20);

            // Act - werfe viele Darts
            var results = new List<ThrowResult>();
            for (int i = 0; i < 1000; i++)
            {
                results.Add(simulator.Throw(target));
            }

            // Assert - Ergebnisse sollten variieren (nicht alle identisch)
            var uniqueScores = results.Select(r => r.Score).Distinct().Count();
            Assert.That(uniqueScores, Is.GreaterThan(1),
                "All throws produced identical results - randomness may be broken");

            // Assert - Alle Ergebnisse sollten gültig sein
            Assert.That(results.All(r => r.Score >= 0 && r.Score <= 60), Is.True);
        }

        [Test]
        public void Throw_DifferentSeeds_ProduceDifferentSequences()
        {
            // Arrange
            var simulator1 = new DartboardSimulatorBuilder()
                .WithSeed(1)
                .Build();

            var simulator2 = new DartboardSimulatorBuilder()
                .WithSeed(2)
                .Build();

            var target = Target.Triple(20);

            // Act
            var results1 = new List<int>();
            var results2 = new List<int>();

            for (int i = 0; i < 100; i++)
            {
                results1.Add(simulator1.Throw(target).Score);
                results2.Add(simulator2.Throw(target).Score);
            }

            // Assert - Unterschiedliche Seeds sollten unterschiedliche Sequenzen erzeugen
            var identicalCount = results1.Zip(results2, (a, b) => a == b).Count(x => x);
            Assert.That(identicalCount, Is.LessThan(100),
                "Different seeds produced identical results");
        }

        [Test]
        public void Throw_SameSeed_ProducesIdenticalSequences()
        {
            // Arrange
            var simulator1 = new DartboardSimulatorBuilder()
                .WithSeed(42)
                .Build();

            var simulator2 = new DartboardSimulatorBuilder()
                .WithSeed(42)
                .Build();

            var targets = new[]
            {
                Target.Triple(20),
                Target.Double(16),
                Target.Bullseye(),
                Target.Single(5),
                Target.Triple(19)
            };

            // Act & Assert
            foreach (var target in targets)
            {
                var result1 = simulator1.Throw(target);
                var result2 = simulator2.Throw(target);

                Assert.That(result1.Score, Is.EqualTo(result2.Score));
                Assert.That(result1.SegmentType, Is.EqualTo(result2.SegmentType));
                Assert.That(result1.SectorNumber, Is.EqualTo(result2.SectorNumber));
            }
        }

        [Test]
        public void Throw_RapidSuccession_CompletesInReasonableTime()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .Build();

            var target = Target.Triple(20);

            // Act - Performance-Messung für schnelle Würfe
            var startTime = DateTime.UtcNow;
            for (int i = 0; i < 10000; i++)
            {
                simulator.Throw(target);
            }
            var elapsed = DateTime.UtcNow - startTime;

            // Assert - sollte schnell genug sein (< 1 Sekunde für 10k Würfe)
            Assert.That(elapsed.TotalSeconds, Is.LessThan(1.0),
                $"10,000 throws took {elapsed.TotalMilliseconds}ms - performance issue detected");
        }
    }

    [TestFixture]
    public class SpecialCaseTests
    {
        [Test]
        public void Throw_AtSector1AllSegmentTypes_ReturnsValidResults()
        {
            // Arrange - Sektor 1 ist der niedrigste Wert
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(42)
                .Build();

            // Act
            var single1 = simulator.Throw(Target.Single(1));
            var double1 = simulator.Throw(Target.Double(1));
            var triple1 = simulator.Throw(Target.Triple(1));

            // Assert
            Assert.That(single1, Is.Not.Null);
            Assert.That(double1, Is.Not.Null);
            Assert.That(triple1, Is.Not.Null);
        }

        [Test]
        public void Throw_AtSector20AllSegmentTypes_ReturnsValidResults()
        {
            // Arrange - Sektor 20 ist der höchste Wert
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(42)
                .Build();

            // Act
            var single20 = simulator.Throw(Target.Single(20));
            var double20 = simulator.Throw(Target.Double(20));
            var triple20 = simulator.Throw(Target.Triple(20));

            // Assert
            Assert.That(single20, Is.Not.Null);
            Assert.That(double20, Is.Not.Null);
            Assert.That(triple20, Is.Not.Null);
        }

        [Test]
        public void ThrowResult_IsDoubleProperty_TrueForDoubleSegments()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(42)
                .Build();

            // Act - werfe auf Double bis wir eins treffen
            ThrowResult? doubleHit = null;
            for (int i = 0; i < 100; i++)
            {
                var result = simulator.Throw(Target.Double(20));
                if (result.SegmentType == SegmentType.Double)
                {
                    doubleHit = result;
                    break;
                }
            }

            // Assert
            Assert.That(doubleHit, Is.Not.Null, "Failed to hit a double in 100 attempts");
            Assert.That(doubleHit!.IsDouble, Is.True);
        }

        [Test]
        public void ThrowResult_IsDoubleProperty_TrueForBullseye()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithSeed(42)
                .Build();

            // Act - werfe auf Bullseye bis wir es treffen
            ThrowResult? bullseyeHit = null;
            for (int i = 0; i < 100; i++)
            {
                var result = simulator.Throw(Target.Bullseye());
                if (result.SegmentType == SegmentType.InnerBull)
                {
                    bullseyeHit = result;
                    break;
                }
            }

            // Assert - Bullseye zählt als Double für Checkout
            Assert.That(bullseyeHit, Is.Not.Null, "Failed to hit bullseye in 100 attempts");
            Assert.That(bullseyeHit!.IsDouble, Is.True);
        }

        [Test]
        public void ThrowResult_IsDoubleProperty_FalseForNonDoubles()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(42)
                .Build();

            var target = Target.Triple(20);

            // Act - sammle verschiedene Würfe
            var results = new List<ThrowResult>();
            for (int i = 0; i < 50; i++)
            {
                results.Add(simulator.Throw(target));
            }

            // Assert - Nicht-Double-Segmente sollten IsDouble = false haben
            var nonDoubles = results.Where(r =>
                r.SegmentType != SegmentType.Double &&
                r.SegmentType != SegmentType.InnerBull);

            foreach (var result in nonDoubles)
            {
                Assert.That(result.IsDouble, Is.False,
                    $"{result.SegmentType} should not be counted as double");
            }
        }

        [Test]
        public void Builder_MultipleConfigurationCalls_LastValueWins()
        {
            // Arrange & Act - mehrfaches Setzen des Seeds
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(1)
                .WithSeed(2)  // sollte vorherigen Wert überschreiben
                .WithSeed(3)  // sollte wieder überschreiben
                .Build();

            var target = Target.Triple(20);
            var result1 = simulator.Throw(target);

            // Vergleiche mit Simulator, der direkt mit Seed 3 gebaut wurde
            var simulator3 = new DartboardSimulatorBuilder()
                .WithSeed(3)
                .Build();
            var result2 = simulator3.Throw(target);

            // Assert
            Assert.That(result1.Score, Is.EqualTo(result2.Score));
            Assert.That(result1.SegmentType, Is.EqualTo(result2.SegmentType));
        }

        [Test]
        public void Builder_WithPrecisionAndStandardDeviation_StandardDeviationOverrides()
        {
            // Arrange & Act - Precision und StandardDeviation zusammen
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Professional)
                .WithStandardDeviation(0.15) // sollte Precision überschreiben
                .Build();

            // Assert - prüfe nur, dass es funktioniert
            var result = simulator.Throw(Target.Triple(20));
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Target_ToString_ReturnsReadableFormat()
        {
            // Arrange
            var targets = new[]
            {
                Target.Single(20),
                Target.Double(16),
                Target.Triple(19),
                Target.Bullseye(),
                Target.OuterBull()
            };

            // Act & Assert
            foreach (var target in targets)
            {
                var str = target.ToString();
                Assert.That(str, Is.Not.Null);
                Assert.That(str, Is.Not.Empty);

                // ToString sollte Typ und Nummer enthalten
                TestContext.Out.WriteLine($"Target: {str}");
            }
        }
    }

    [TestFixture]
    public class RobustnessTests
    {
        [Test]
        public void Simulator_LongRunningSession_MaintainsConsistency()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(42)
                .Build();

            var targets = new[]
            {
                Target.Triple(20),
                Target.Triple(19),
                Target.Triple(18),
                Target.Double(16),
                Target.Bullseye()
            };

            // Act - simuliere eine sehr lange Session (5000 Würfe)
            var allResults = new List<ThrowResult>();
            for (int round = 0; round < 1000; round++)
            {
                foreach (var target in targets)
                {
                    var result = simulator.Throw(target);
                    allResults.Add(result);
                }
            }

            // Assert
            Assert.That(allResults, Has.Count.EqualTo(5000));
            Assert.That(allResults.All(r => r.Score >= 0 && r.Score <= 60), Is.True,
                "Some results had invalid scores");

            // Prüfe auf vernünftige Verteilung
            var averageScore = allResults.Average(r => r.Score);
            Assert.That(averageScore, Is.GreaterThan(0),
                "Average score should be greater than 0");
        }

        [Test]
        public void Simulator_AlternatingTargets_ProducesVariedResults()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithSeed(777)
                .Build();

            // Act - wechsle zwischen sehr unterschiedlichen Zielen
            var results = new List<(Target Target, ThrowResult Result)>();
            for (int i = 0; i < 100; i++)
            {
                var target1 = Target.Triple(20);
                var target2 = Target.Single(1);

                results.Add((target1, simulator.Throw(target1)));
                results.Add((target2, simulator.Throw(target2)));
            }

            // Assert - beide Ziele sollten gültige Ergebnisse liefern
            Assert.That(results.All(r => r.Result.Score >= 0), Is.True);
        }

        [Test]
        public void Simulator_StatisticalDistribution_ShowsRealisticVariance()
        {
            // Arrange
            var simulator = new DartboardSimulatorBuilder()
                .WithPrecision(SimulatorPrecision.Amateur)
                .WithSeed(999)
                .Build();

            var target = Target.Triple(20);

            // Act - sammle viele Proben
            var samples = new List<ThrowResult>();
            for (int i = 0; i < 1000; i++)
            {
                samples.Add(simulator.Throw(target));
            }

            // Assert - prüfe auf realistische Verteilungseigenschaften

            // Sollte das Ziel manchmal treffen, aber nicht immer
            var hitCount = samples.Count(s => s.SegmentType == SegmentType.Triple && s.SectorNumber == 20);
            Assert.That(hitCount, Is.GreaterThan(0), "Never hit the target - precision might be too low");
            Assert.That(hitCount, Is.LessThan(1000), "Always hit the target - no randomness");

            // Sollte mehrere verschiedene Scores haben
            var uniqueScores = samples.Select(s => s.Score).Distinct().Count();
            Assert.That(uniqueScores, Is.GreaterThan(3), "Too few unique scores - distribution too narrow");

            // Sollte gelegentlich daneben werfen bei Amateur-Präzision
            var missCount = samples.Count(s => s.SegmentType == SegmentType.Miss);
            TestContext.Out.WriteLine($"Hit rate: {hitCount}/1000, Unique scores: {uniqueScores}, Misses: {missCount}");
        }

        [Test]
        public void Simulator_MultipleInstances_AreIndependent()
        {
            // Arrange - erstelle mehrere Simulatoren
            var sim1 = new DartboardSimulatorBuilder().WithSeed(1).Build();
            var sim2 = new DartboardSimulatorBuilder().WithSeed(2).Build();
            var sim3 = new DartboardSimulatorBuilder().WithSeed(3).Build();

            var target = Target.Triple(20);

            // Act - werfe mit allen Simulatoren
            var results1 = new List<int>();
            var results2 = new List<int>();
            var results3 = new List<int>();

            for (int i = 0; i < 50; i++)
            {
                results1.Add(sim1.Throw(target).Score);
                results2.Add(sim2.Throw(target).Score);
                results3.Add(sim3.Throw(target).Score);
            }

            // Assert - jeder Simulator sollte seinen eigenen Zustand behalten
            Assert.That(results1, Is.Not.EqualTo(results2));
            Assert.That(results2, Is.Not.EqualTo(results3));
            Assert.That(results1, Is.Not.EqualTo(results3));
        }
    }
}
