using Dartillery;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
public class EnhancedSimulatorAnalysisTests
{
    [TestFixture]
    public class TremorAnalysisTests
    {
        [Test]
        public void AnalyzeTremorImpact_LinearModel_ShowsPerformanceDegradation()
        {
            // Arrange
            const int throwsPerCheckpoint = 50;
            const int checkpoints = 10;
            var target = Target.Triple(20);

            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer("TremorTest")
                .WithLinearTremor()
                .WithSeed(42)
                .BuildSession();

            Console.WriteLine("=== Tremor Impact Analysis on T20 (Linear Model) ===\n");
            Console.WriteLine($"{"Checkpoint",-12} {"Throws",-8} {"Tremor",-12} {"Avg Score",-12} {"Target Hit %",-12}");
            Console.WriteLine(new string('-', 60));

            // Act & Assert
            for (int checkpoint = 0; checkpoint < checkpoints; checkpoint++)
            {
                var scores = new List<int>();
                var hits = 0;

                for (int i = 0; i < throwsPerCheckpoint; i++)
                {
                    var result = session.Throw(target);
                    scores.Add(result.Score);
                    if (result.SegmentType == SegmentType.Triple && result.SectorNumber == 20)
                        hits++;
                }

                var avgScore = scores.Average();
                var hitRate = hits * 100.0 / throwsPerCheckpoint;
                var tremor = session.CurrentTremor;
                var totalThrows = (checkpoint + 1) * throwsPerCheckpoint;

                Console.WriteLine($"{checkpoint + 1,-12} {totalThrows,-8} {tremor,-12:F6} {avgScore,-12:F2} {hitRate,-11:F1}%");
            }

            // Tremor sollte deutlich angestiegen sein
            Assert.That(session.CurrentTremor, Is.GreaterThan(0.01));
        }

        [Test]
        public void AnalyzeTremorImpact_RealisticVsLinear_CompareModels()
        {
            // Arrange
            const int totalThrows = 500;
            var target = Target.Triple(20);

            var linearSession = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithLinearTremor()
                .WithSeed(42)
                .BuildSession();

            var realisticSession = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithRealisticTremor()
                .WithSeed(42)
                .BuildSession();

            Console.WriteLine("=== Tremor Model Comparison ===\n");
            Console.WriteLine($"{"Throws",-10} {"Linear",-15} {"Realistic",-15}");
            Console.WriteLine(new string('-', 45));

            var checkpoints = new[] { 50, 100, 200, 300, 400, 500 };

            for (int i = 0; i < totalThrows; i++)
            {
                linearSession.Throw(target);
                realisticSession.Throw(target);

                if (checkpoints.Contains(i + 1))
                {
                    Console.WriteLine($"{i + 1,-10} {linearSession.CurrentTremor,-15:F6} {realisticSession.CurrentTremor,-15:F6}");
                }
            }

            Assert.That(linearSession.CurrentTremor, Is.GreaterThan(0));
            Assert.That(realisticSession.CurrentTremor, Is.GreaterThan(0));
        }

        [Test]
        public void AnalyzeFatigueRecovery_AfterReset_ShowsRecovery()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithAmateurPlayer()
                .WithLinearTremor()
                .WithSeed(42)
                .BuildSession();

            Console.WriteLine("=== Fatigue and Recovery Analysis ===\n");

            // Act - Spiel 1
            for (int i = 0; i < 100; i++)
            {
                session.Throw(Target.Triple(20));
            }
            var tremorAfterGame1 = session.CurrentTremor;
            Console.WriteLine($"After Game 1 (100 throws): Tremor = {tremorAfterGame1:F6}");

            // Reset (Pause)
            session.Reset();
            var tremorAfterReset = session.CurrentTremor;
            Console.WriteLine($"After Reset: Tremor = {tremorAfterReset:F6}");

            // Spiel 2
            for (int i = 0; i < 100; i++)
            {
                session.Throw(Target.Triple(20));
            }
            var tremorAfterGame2 = session.CurrentTremor;
            Console.WriteLine($"After Game 2 (100 throws): Tremor = {tremorAfterGame2:F6}");

            // Assert - Reset sollte Tremor zurücksetzen
            Assert.That(tremorAfterReset, Is.LessThan(tremorAfterGame1));
        }
    }

    [TestFixture]
    public class PressureAnalysisTests
    {
        [Test]
        public void AnalyzePressure_CheckoutVsNormal_ShowsImpact()
        {
            // Arrange
            const int throwsPerScenario = 200;
            var target = Target.Double(20);

            var session = new EnhancedDartboardSimulatorBuilder()
                .WithAmateurPlayer()
                .WithCheckoutPsychology()
                .WithSeed(42)
                .BuildSession();

            Console.WriteLine("=== Pressure Impact Analysis ===\n");

            var normalScores = new List<int>();
            var normalHits = 0;

            for (int i = 0; i < throwsPerScenario; i++)
            {
                var result = session.Throw(target);
                normalScores.Add(result.Score);
                if (result.SegmentType == SegmentType.Double && result.SectorNumber == 20)
                    normalHits++;
            }

            session.Reset();

            // Act - Checkout-Druck
            var checkoutContext = new GameContext
            {
                RemainingScore = 40,
                CurrentVisitResults = new List<ThrowResult>()
            };

            var pressureScores = new List<int>();
            var pressureHits = 0;

            for (int i = 0; i < throwsPerScenario; i++)
            {
                var result = session.Throw(target, checkoutContext);
                pressureScores.Add(result.Score);
                if (result.SegmentType == SegmentType.Double && result.SectorNumber == 20)
                    pressureHits++;
            }

            // Assert & Report
            var normalAvg = normalScores.Average();
            var normalHitRate = normalHits * 100.0 / throwsPerScenario;

            var pressureAvg = pressureScores.Average();
            var pressureHitRate = pressureHits * 100.0 / throwsPerScenario;

            Console.WriteLine($"Normal Situation:");
            Console.WriteLine($"  Average Score: {normalAvg:F2}");
            Console.WriteLine($"  Hit Rate (D20): {normalHitRate:F1}%");
            Console.WriteLine();
            Console.WriteLine($"Checkout Pressure:");
            Console.WriteLine($"  Average Score: {pressureAvg:F2}");
            Console.WriteLine($"  Hit Rate (D20): {pressureHitRate:F1}%");
            Console.WriteLine();
            Console.WriteLine($"Impact:");
            Console.WriteLine($"  Score Difference: {pressureAvg - normalAvg:F2} ({(pressureAvg - normalAvg) / normalAvg * 100:F1}%)");
            Console.WriteLine($"  Hit Rate Difference: {pressureHitRate - normalHitRate:F1}%");

            Assert.That(pressureScores.All(s => s >= 0 && s <= 40), Is.True);
            Assert.That(normalScores.All(s => s >= 0 && s <= 40), Is.True);
        }

        [Test]
        public void AnalyzePressureResistance_DifferentProfiles_ShowsVariation()
        {
            // Arrange
            const int throwsPerPlayer = 100;
            var checkoutContext = new GameContext
            {
                RemainingScore = 32,
                CurrentVisitResults = new List<ThrowResult>()
            };

            var profiles = new[]
            {
                ("Professional", new EnhancedDartboardSimulatorBuilder()
                    .WithProfessionalPlayer()
                    .WithStandardPressure()
                    .WithSeed(42)
                    .BuildSession()),
                ("Amateur", new EnhancedDartboardSimulatorBuilder()
                    .WithAmateurPlayer()
                    .WithStandardPressure()
                    .WithSeed(42)
                    .BuildSession()),
                ("Beginner", new EnhancedDartboardSimulatorBuilder()
                    .WithBeginnerPlayer()
                    .WithStandardPressure()
                    .WithSeed(42)
                    .BuildSession())
            };

            Console.WriteLine("=== Pressure Resistance by Player Level ===\n");
            Console.WriteLine($"{"Level",-15} {"Resistance",-15} {"Avg Score",-12} {"Hit %",-12}");
            Console.WriteLine(new string('-', 60));

            // Act & Assert
            foreach (var (name, session) in profiles)
            {
                var scores = new List<int>();
                var hits = 0;

                for (int i = 0; i < throwsPerPlayer; i++)
                {
                    var result = session.Throw(Target.Double(16), checkoutContext);
                    scores.Add(result.Score);
                    if (result.SegmentType == SegmentType.Double && result.SectorNumber == 16)
                        hits++;
                }

                var avgScore = scores.Average();
                var hitRate = hits * 100.0 / throwsPerPlayer;
                var resistance = session.Profile.PressureResistance;

                Console.WriteLine($"{name,-15} {resistance,-15:F2} {avgScore,-12:F2} {hitRate,-11:F1}%");
            }
        }
    }

    [TestFixture]
    public class PlayerProfileComparisonTests
    {
        [Test]
        public void ComparePlayerLevels_MultipleTargets_ShowsSkillDifferences()
        {
            // Arrange
            const int throwsPerTarget = 200;
            var targets = new[]
            {
                ("T20", Target.Triple(20)),
                ("D20", Target.Double(20)),
                ("Bull", Target.Bullseye()),
                ("S20", Target.Single(20))
            };

            var players = new[]
            {
                ("Pro", new EnhancedDartboardSimulatorBuilder()
                    .WithProfessionalPlayer()
                    .WithSeed(42)
                    .BuildSession()),
                ("Amateur", new EnhancedDartboardSimulatorBuilder()
                    .WithAmateurPlayer()
                    .WithSeed(42)
                    .BuildSession()),
                ("Beginner", new EnhancedDartboardSimulatorBuilder()
                    .WithBeginnerPlayer()
                    .WithSeed(42)
                    .BuildSession())
            };

            Console.WriteLine("=== Player Level Comparison Across Targets ===\n");

            foreach (var (targetName, target) in targets)
            {
                Console.WriteLine($"Target: {targetName}");
                Console.WriteLine($"{"Player",-12} {"Avg Score",-12} {"Max Score",-12} {"Hit %",-12}");
                Console.WriteLine(new string('-', 50));

                foreach (var (playerName, session) in players)
                {
                    session.Reset();

                    var scores = new List<int>();
                    var hits = 0;

                    for (int i = 0; i < throwsPerTarget; i++)
                    {
                        var result = session.Throw(target);
                        scores.Add(result.Score);

                        // Zähle exakte Treffer
                        bool isHit = target.SegmentType switch
                        {
                            SegmentType.InnerBull => result.SegmentType == SegmentType.InnerBull,
                            SegmentType.OuterBull => result.SegmentType == SegmentType.OuterBull,
                            _ => result.SegmentType == target.SegmentType && result.SectorNumber == target.SectorNumber
                        };

                        if (isHit) hits++;
                    }

                    var avgScore = scores.Average();
                    var maxScore = scores.Max();
                    var hitRate = hits * 100.0 / throwsPerTarget;

                    Console.WriteLine($"{playerName,-12} {avgScore,-12:F2} {maxScore,-12} {hitRate,-11:F1}%");
                }
                Console.WriteLine();
            }
        }

        [Test]
        public void Analyze3DartAverage_ExtendedSession_ShowsTrends()
        {
            // Arrange
            const int totalVisits = 50;
            var target = Target.Triple(20);

            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithLinearTremor()
                .WithSeed(42)
                .BuildSession();

            Console.WriteLine("=== 3-Dart Average Progression ===\n");
            Console.WriteLine($"{"Visit",-8} {"Dart 1",-8} {"Dart 2",-8} {"Dart 3",-8} {"Total",-8} {"Avg",-10}");
            Console.WriteLine(new string('-', 60));

            var allVisitScores = new List<int>();

            // Act
            for (int visit = 0; visit < totalVisits; visit++)
            {
                int visitScore = 0;
                var dartScores = new int[3];

                for (int dart = 0; dart < 3; dart++)
                {
                    var result = session.Throw(target);
                    dartScores[dart] = result.Score;
                    visitScore += result.Score;
                }

                allVisitScores.Add(visitScore);

                if (visit < 10 || visit % 5 == 0)
                {
                    var runningAvg = allVisitScores.Average();
                    Console.WriteLine($"{visit + 1,-8} {dartScores[0],-8} {dartScores[1],-8} {dartScores[2],-8} {visitScore,-8} {runningAvg,-10:F2}");
                }
            }

            // Assert
            var overallAvg = allVisitScores.Average();
            Console.WriteLine($"\nOverall 3-dart average: {overallAvg:F2}");
            Console.WriteLine($"Final tremor level: {session.CurrentTremor:F6}");

            Assert.That(overallAvg, Is.GreaterThan(0));
        }
    }

    [TestFixture]
    public class FeatureCombinationTests
    {
        [Test]
        public void AnalyzeFullFeatureSet_Complete501Game_DetailedStatistics()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer("FullFeatureTest")
                .WithRealisticTremor()
                .WithCheckoutPsychology()
                .WithStandardMomentum()
                .WithSimpleGrouping()
                .WithStandardTargetDifficulty()
                .WithTruncation(0.25)
                .WithSeed(42)
                .BuildSession();

            int remainingScore = 501;
            var visitStats = new List<(int Visit, int Score, int Darts, int Remaining, double Tremor)>();

            Console.WriteLine("=== Full Feature 501 Game Analysis ===\n");
            Console.WriteLine($"{"Visit",-8} {"Darts",-8} {"Score",-8} {"Remain",-10} {"Tremor",-12}");
            Console.WriteLine(new string('-', 55));

            int visitNumber = 0;

            // Act
            while (remainingScore > 0 && session.ThrowCount < 150)
            {
                visitNumber++;
                var gameContext = new GameContext
                {
                    RemainingScore = remainingScore,
                    CurrentVisitResults = new List<ThrowResult>()
                };

                int visitScore = 0;
                int dartsInVisit = 0;
                bool busted = false;

                for (int dart = 0; dart < 3 && remainingScore > 0; dart++)
                {
                    var target = SelectSmartTarget(remainingScore);
                    var result = session.Throw(target, gameContext);

                    gameContext.CurrentVisitResults.Add(result);
                    dartsInVisit++;

                    var newRemaining = remainingScore - visitScore - result.Score;

                    // Bust-Prüfung
                    if (newRemaining == 1 || newRemaining < 0 ||
                        (newRemaining == 0 && !result.IsDouble))
                    {
                        busted = true;
                        break;
                    }

                    visitScore += result.Score;

                    // Erfolgreicher Checkout
                    if (newRemaining == 0 && result.IsDouble)
                    {
                        remainingScore = 0;
                        break;
                    }
                }

                if (!busted && remainingScore > 0)
                {
                    remainingScore -= visitScore;
                }

                var tremor = session.CurrentTremor;
                visitStats.Add((visitNumber, visitScore, dartsInVisit, remainingScore, tremor));

                if (visitNumber <= 10 || visitNumber % 5 == 0 || remainingScore == 0)
                {
                    Console.WriteLine($"{visitNumber,-8} {dartsInVisit,-8} {visitScore,-8} {remainingScore,-10} {tremor,-12:F6}");
                }

                if (remainingScore == 0)
                    break;
            }

            // Assert & Summary
            var totalDarts = session.ThrowCount;
            var totalScore = 501 - remainingScore;
            var threeDartAvg = totalScore * 3.0 / totalDarts;

            Console.WriteLine();
            Console.WriteLine("=== Game Summary ===");
            Console.WriteLine($"Game finished: {(remainingScore == 0 ? "YES" : "NO")}");
            Console.WriteLine($"Total darts: {totalDarts}");
            Console.WriteLine($"Total visits: {visitNumber}");
            Console.WriteLine($"3-dart average: {threeDartAvg:F2}");
            Console.WriteLine($"Final tremor: {session.CurrentTremor:F6}");

            Assert.That(totalDarts, Is.GreaterThan(0));
        }

        [Test]
        public void CompareFeatureSets_WithAndWithoutEnhancements_ShowsDifference()
        {
            // Arrange
            const int throwsPerSession = 300;
            var target = Target.Triple(20);

            var basic = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithSeed(42)
                .BuildSession();

            var enhanced = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithRealisticTremor()
                .WithStandardPressure()
                .WithStandardMomentum()
                .WithSimpleGrouping()
                .WithStandardTargetDifficulty()
                .WithSeed(42)
                .BuildSession();

            Console.WriteLine("=== Feature Set Comparison ===\n");
            Console.WriteLine($"{"Session",-15} {"Avg Score",-12} {"Hits",-8} {"Hit %",-10}");
            Console.WriteLine(new string('-', 50));

            // Act - Basic
            var basicScores = new List<int>();
            var basicHits = 0;

            for (int i = 0; i < throwsPerSession; i++)
            {
                var result = basic.Throw(target);
                basicScores.Add(result.Score);
                if (result.SegmentType == SegmentType.Triple && result.SectorNumber == 20)
                    basicHits++;
            }

            // Act - Enhanced
            var enhancedScores = new List<int>();
            var enhancedHits = 0;

            for (int i = 0; i < throwsPerSession; i++)
            {
                var result = enhanced.Throw(target);
                enhancedScores.Add(result.Score);
                if (result.SegmentType == SegmentType.Triple && result.SectorNumber == 20)
                    enhancedHits++;
            }

            // Assert & Report
            var basicAvg = basicScores.Average();
            var basicHitRate = basicHits * 100.0 / throwsPerSession;

            var enhancedAvg = enhancedScores.Average();
            var enhancedHitRate = enhancedHits * 100.0 / throwsPerSession;

            Console.WriteLine($"{"Basic",-15} {basicAvg,-12:F2} {basicHits,-8} {basicHitRate,-9:F1}%");
            Console.WriteLine($"{"Enhanced",-15} {enhancedAvg,-12:F2} {enhancedHits,-8} {enhancedHitRate,-9:F1}%");

            Assert.That(basicAvg, Is.GreaterThan(0));
            Assert.That(enhancedAvg, Is.GreaterThan(0));
        }

        private Target SelectSmartTarget(int remaining)
        {
            // Einfache aber intelligente Strategie
            if (remaining <= 40 && remaining % 2 == 0 && remaining >= 2)
            {
                int doubleTarget = remaining / 2;
                if (doubleTarget <= 20)
                    return Target.Double(doubleTarget);
            }

            if (remaining == 50)
                return Target.Bullseye();

            // Unter 60: auf Single zielen um Setup zu schaffen
            if (remaining < 60 && remaining % 2 != 0)
                return Target.Single(1);

            // Standard: T20
            return Target.Triple(20);
        }
    }

    [TestFixture]
    public class GroupingAnalysisTests
    {
        [Test]
        public void AnalyzeGrouping_ConsecutiveThrowsSameTarget_ShowsClustering()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithSimpleGrouping()
                .WithSeed(42)
                .BuildSession();

            var target = Target.Triple(20);
            const int throwCount = 100;

            Console.WriteLine("=== Grouping Analysis ===\n");

            // Act
            var coordinates = new List<(double X, double Y)>();
            for (int i = 0; i < throwCount; i++)
            {
                var result = session.Throw(target);
                coordinates.Add((result.HitPoint.X, result.HitPoint.Y));
            }

            // Berechne Streuung
            var avgX = coordinates.Average(c => c.X);
            var avgY = coordinates.Average(c => c.Y);

            var variance = coordinates.Sum(c =>
            {
                var dx = c.X - avgX;
                var dy = c.Y - avgY;
                return dx * dx + dy * dy;
            }) / throwCount;

            var stdDev = Math.Sqrt(variance);

            Console.WriteLine($"Throws: {throwCount}");
            Console.WriteLine($"Center: ({avgX:F6}, {avgY:F6})");
            Console.WriteLine($"Standard Deviation: {stdDev:F6}");

            // Assert
            Assert.That(coordinates, Has.Count.EqualTo(throwCount));
            Assert.That(stdDev, Is.GreaterThan(0));
        }
    }
}
