using Dartillery;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
[Category("Analysis")]
public class EnhancedSimulatorAnalysisTests
{
    internal static PlayerSession CreateSession(
        Action<EnhancedDartboardSimulatorBuilder>? configure = null,
        int seed = 42)
    {
        var builder = new EnhancedDartboardSimulatorBuilder().WithSeed(seed);
        configure?.Invoke(builder);
        return builder.BuildSession();
    }

    [TestFixture]
    public class FatigueAnalysisTests
    {
        [Test]
        public void Throw_LinearFatigueOverManyThrows_FatigueExceedsThreshold()
        {
            const int throwsPerCheckpoint = 50;
            const int checkpoints = 10;
            var target = Target.Triple(20);

            var session = EnhancedSimulatorAnalysisTests.CreateSession(b => b
                .WithProfessionalPlayer("FatigueTest")
                .WithLinearFatigue());

            TestContext.Out.WriteLine("=== Fatigue Impact Analysis on T20 (Linear Model) ===\n");
            TestContext.Out.WriteLine($"{"Checkpoint",-12} {"Throws",-8} {"Fatigue",-12} {"Avg Score",-12} {"Target Hit %",-12}");
            TestContext.Out.WriteLine(new string('-', 60));

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
                var fatigue = session.CurrentFatigue;
                var totalThrows = (checkpoint + 1) * throwsPerCheckpoint;

                TestContext.Out.WriteLine($"{checkpoint + 1,-12} {totalThrows,-8} {fatigue,-12:F6} {avgScore,-12:F2} {hitRate,-11:F1}%");
            }

            Assert.That(session.CurrentFatigue, Is.GreaterThan(0.01));
        }

        [Test]
        public void Throw_RealisticVsLinearFatigue_BothAccumulatePositively()
        {
            const int totalThrows = 500;
            var target = Target.Triple(20);

            var linearSession = EnhancedSimulatorAnalysisTests.CreateSession(b => b
                .WithProfessionalPlayer()
                .WithLinearFatigue());

            var realisticSession = EnhancedSimulatorAnalysisTests.CreateSession(b => b
                .WithProfessionalPlayer()
                .WithRealisticFatigue());

            TestContext.Out.WriteLine("=== Fatigue Model Comparison ===\n");
            TestContext.Out.WriteLine($"{"Throws",-10} {"Linear",-15} {"Realistic",-15}");
            TestContext.Out.WriteLine(new string('-', 45));

            var checkpoints = new[] { 50, 100, 200, 300, 400, 500 };

            for (int i = 0; i < totalThrows; i++)
            {
                linearSession.Throw(target);
                realisticSession.Throw(target);

                if (checkpoints.Contains(i + 1))
                {
                    TestContext.Out.WriteLine($"{i + 1,-10} {linearSession.CurrentFatigue,-15:F6} {realisticSession.CurrentFatigue,-15:F6}");
                }
            }

            Assert.That(linearSession.CurrentFatigue, Is.GreaterThan(0));
            Assert.That(realisticSession.CurrentFatigue, Is.GreaterThan(0));
        }

        [Test]
        public void Reset_AfterFatigue_FatigueDecreases()
        {
            var session = EnhancedSimulatorAnalysisTests.CreateSession(b => b
                .WithAmateurPlayer()
                .WithLinearFatigue());

            TestContext.Out.WriteLine("=== Fatigue and Recovery Analysis ===\n");

            for (int i = 0; i < 100; i++)
            {
                session.Throw(Target.Triple(20));
            }

            var fatigueAfterGame1 = session.CurrentFatigue;
            TestContext.Out.WriteLine($"After Game 1 (100 throws): Fatigue = {fatigueAfterGame1:F6}");

            // Reset (Pause)
            session.Reset();
            var fatigueAfterReset = session.CurrentFatigue;
            TestContext.Out.WriteLine($"After Reset: Fatigue = {fatigueAfterReset:F6}");

            // Game 2
            for (int i = 0; i < 100; i++)
            {
                session.Throw(Target.Triple(20));
            }

            var fatigueAfterGame2 = session.CurrentFatigue;
            TestContext.Out.WriteLine($"After Game 2 (100 throws): Fatigue = {fatigueAfterGame2:F6}");

            Assert.That(fatigueAfterReset, Is.LessThan(fatigueAfterGame1));
        }
    }

    [TestFixture]
    public class PressureAnalysisTests
    {
        [Test]
        public void Throw_CheckoutPressureVsNormal_AllScoresInValidRange()
        {
            const int throwsPerScenario = 200;
            var target = Target.Double(20);

            var session = EnhancedSimulatorAnalysisTests.CreateSession(b => b
                .WithAmateurPlayer()
                .WithCheckoutPsychology());

            TestContext.Out.WriteLine("=== Pressure Impact Analysis ===\n");

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

            var normalAvg = normalScores.Average();
            var normalHitRate = normalHits * 100.0 / throwsPerScenario;

            var pressureAvg = pressureScores.Average();
            var pressureHitRate = pressureHits * 100.0 / throwsPerScenario;

            TestContext.Out.WriteLine($"Normal Situation:");
            TestContext.Out.WriteLine($"  Average Score: {normalAvg:F2}");
            TestContext.Out.WriteLine($"  Hit Rate (D20): {normalHitRate:F1}%");
            TestContext.Out.WriteLine();
            TestContext.Out.WriteLine($"Checkout Pressure:");
            TestContext.Out.WriteLine($"  Average Score: {pressureAvg:F2}");
            TestContext.Out.WriteLine($"  Hit Rate (D20): {pressureHitRate:F1}%");
            TestContext.Out.WriteLine();
            TestContext.Out.WriteLine($"Impact:");
            TestContext.Out.WriteLine($"  Score Difference: {pressureAvg - normalAvg:F2} ({(pressureAvg - normalAvg) / normalAvg * 100:F1}%)");
            TestContext.Out.WriteLine($"  Hit Rate Difference: {pressureHitRate - normalHitRate:F1}%");

            Assert.That(pressureScores.All(s => s >= 0 && s <= 40), Is.True);
            Assert.That(normalScores.All(s => s >= 0 && s <= 40), Is.True);
        }

        [Test]
        public void Throw_AllProfilesUnderPressure_ScoresAndHitRatesPositive()
        {
            const int throwsPerPlayer = 100;
            var checkoutContext = new GameContext
            {
                RemainingScore = 32,
                CurrentVisitResults = new List<ThrowResult>()
            };

            var profiles = new[]
            {
                ("Professional", EnhancedSimulatorAnalysisTests.CreateSession(b => b.WithProfessionalPlayer().WithStandardPressure())),
                ("Amateur", EnhancedSimulatorAnalysisTests.CreateSession(b => b.WithAmateurPlayer().WithStandardPressure())),
                ("Beginner", EnhancedSimulatorAnalysisTests.CreateSession(b => b.WithBeginnerPlayer().WithStandardPressure()))
            };

            TestContext.Out.WriteLine("=== Pressure Resistance by Player Level ===\n");
            TestContext.Out.WriteLine($"{"Level",-15} {"Resistance",-15} {"Avg Score",-12} {"Hit %",-12}");
            TestContext.Out.WriteLine(new string('-', 60));

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

                TestContext.Out.WriteLine($"{name,-15} {resistance,-15:F2} {avgScore,-12:F2} {hitRate,-11:F1}%");

                Assert.That(avgScore, Is.GreaterThanOrEqualTo(0));
                Assert.That(hitRate, Is.GreaterThanOrEqualTo(0));
            }
        }
    }

    [TestFixture]
    public class PlayerProfileComparisonTests
    {
        [Test]
        public void Throw_ThreeProfilesAcrossTargets_AllProduceValidResults()
        {
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
                ("Pro", EnhancedSimulatorAnalysisTests.CreateSession(b => b.WithProfessionalPlayer())),
                ("Amateur", EnhancedSimulatorAnalysisTests.CreateSession(b => b.WithAmateurPlayer())),
                ("Beginner", EnhancedSimulatorAnalysisTests.CreateSession(b => b.WithBeginnerPlayer()))
            };

            TestContext.Out.WriteLine("=== Player Level Comparison Across Targets ===\n");

            foreach (var (targetName, target) in targets)
            {
                TestContext.Out.WriteLine($"Target: {targetName}");
                TestContext.Out.WriteLine($"{"Player",-12} {"Avg Score",-12} {"Max Score",-12} {"Hit %",-12}");
                TestContext.Out.WriteLine(new string('-', 50));

                foreach (var (playerName, session) in players)
                {
                    session.Reset();

                    var scores = new List<int>();
                    var hits = 0;

                    for (int i = 0; i < throwsPerTarget; i++)
                    {
                        var result = session.Throw(target);
                        scores.Add(result.Score);

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

                    TestContext.Out.WriteLine($"{playerName,-12} {avgScore,-12:F2} {maxScore,-12} {hitRate,-11:F1}%");

                    Assert.That(avgScore, Is.GreaterThanOrEqualTo(0));
                }

                TestContext.Out.WriteLine();
            }
        }

        [Test]
        public void Throw_ProfessionalWith50Visits_AverageScorePositive()
        {
            const int totalVisits = 50;
            var target = Target.Triple(20);

            var session = EnhancedSimulatorAnalysisTests.CreateSession(b => b
                .WithProfessionalPlayer()
                .WithLinearFatigue());

            TestContext.Out.WriteLine("=== 3-Dart Average Progression ===\n");
            TestContext.Out.WriteLine($"{"Visit",-8} {"Dart 1",-8} {"Dart 2",-8} {"Dart 3",-8} {"Total",-8} {"Avg",-10}");
            TestContext.Out.WriteLine(new string('-', 60));

            var allVisitScores = new List<int>();

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
                    TestContext.Out.WriteLine($"{visit + 1,-8} {dartScores[0],-8} {dartScores[1],-8} {dartScores[2],-8} {visitScore,-8} {runningAvg,-10:F2}");
                }
            }

            var overallAvg = allVisitScores.Average();
            TestContext.Out.WriteLine($"\nOverall 3-dart average: {overallAvg:F2}");
            TestContext.Out.WriteLine($"Final fatigue level: {session.CurrentFatigue:F6}");

            Assert.That(overallAvg, Is.GreaterThan(0));
        }
    }

    [TestFixture]
    public class FeatureCombinationTests
    {
        [Test]
        public void Throw_FullFeatureSetIn501Game_TotalDartsPositive()
        {
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer("FullFeatureTest")
                .WithRealisticFatigue()
                .WithCheckoutPsychology()
                .WithStandardMomentum()
                .WithSimpleGrouping()
                .WithStandardTargetDifficulty()
                .WithTruncation(0.25)
                .WithSeed(42)
                .BuildSession();

            int remainingScore = 501;
            var visitStats = new List<(int Visit, int Score, int Darts, int Remaining, double Fatigue)>();

            TestContext.Out.WriteLine("=== Full Feature 501 Game Analysis ===\n");
            TestContext.Out.WriteLine($"{"Visit",-8} {"Darts",-8} {"Score",-8} {"Remain",-10} {"Fatigue",-12}");
            TestContext.Out.WriteLine(new string('-', 55));

            int visitNumber = 0;

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

                    // Bust check
                    if (newRemaining == 1 || newRemaining < 0 ||
                        (newRemaining == 0 && !result.IsDouble))
                    {
                        busted = true;
                        break;
                    }

                    visitScore += result.Score;

                    // Successful checkout
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

                var fatigue = session.CurrentFatigue;
                visitStats.Add((visitNumber, visitScore, dartsInVisit, remainingScore, fatigue));

                if (visitNumber <= 10 || visitNumber % 5 == 0 || remainingScore == 0)
                {
                    TestContext.Out.WriteLine($"{visitNumber,-8} {dartsInVisit,-8} {visitScore,-8} {remainingScore,-10} {fatigue,-12:F6}");
                }

                if (remainingScore == 0)
                    break;
            }

            var totalDarts = session.ThrowCount;
            var totalScore = 501 - remainingScore;
            var threeDartAvg = totalScore * 3.0 / totalDarts;

            TestContext.Out.WriteLine();
            TestContext.Out.WriteLine("=== Game Summary ===");
            TestContext.Out.WriteLine($"Game finished: {(remainingScore == 0 ? "YES" : "NO")}");
            TestContext.Out.WriteLine($"Total darts: {totalDarts}");
            TestContext.Out.WriteLine($"Total visits: {visitNumber}");
            TestContext.Out.WriteLine($"3-dart average: {threeDartAvg:F2}");
            TestContext.Out.WriteLine($"Final fatigue: {session.CurrentFatigue:F6}");

            Assert.That(totalDarts, Is.GreaterThan(0));
        }

        [Test]
        public void Throw_BasicVsEnhancedSession_BothAveragesPositive()
        {
            const int throwsPerSession = 300;
            var target = Target.Triple(20);

            var basic = EnhancedSimulatorAnalysisTests.CreateSession(b => b.WithProfessionalPlayer());

            var enhanced = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithRealisticFatigue()
                .WithStandardPressure()
                .WithStandardMomentum()
                .WithSimpleGrouping()
                .WithStandardTargetDifficulty()
                .WithSeed(42)
                .BuildSession();

            TestContext.Out.WriteLine("=== Feature Set Comparison ===\n");
            TestContext.Out.WriteLine($"{"Session",-15} {"Avg Score",-12} {"Hits",-8} {"Hit %",-10}");
            TestContext.Out.WriteLine(new string('-', 50));

            var basicScores = new List<int>();
            var basicHits = 0;

            for (int i = 0; i < throwsPerSession; i++)
            {
                var result = basic.Throw(target);
                basicScores.Add(result.Score);
                if (result.SegmentType == SegmentType.Triple && result.SectorNumber == 20)
                    basicHits++;
            }

            var enhancedScores = new List<int>();
            var enhancedHits = 0;

            for (int i = 0; i < throwsPerSession; i++)
            {
                var result = enhanced.Throw(target);
                enhancedScores.Add(result.Score);
                if (result.SegmentType == SegmentType.Triple && result.SectorNumber == 20)
                    enhancedHits++;
            }

            var basicAvg = basicScores.Average();
            var basicHitRate = basicHits * 100.0 / throwsPerSession;

            var enhancedAvg = enhancedScores.Average();
            var enhancedHitRate = enhancedHits * 100.0 / throwsPerSession;

            TestContext.Out.WriteLine($"{"Basic",-15} {basicAvg,-12:F2} {basicHits,-8} {basicHitRate,-9:F1}%");
            TestContext.Out.WriteLine($"{"Enhanced",-15} {enhancedAvg,-12:F2} {enhancedHits,-8} {enhancedHitRate,-9:F1}%");

            Assert.That(basicAvg, Is.GreaterThan(0));
            Assert.That(enhancedAvg, Is.GreaterThan(0));
        }

        [Test]
        public void Throw_GroupingOnSameTarget_VariancePositive()
        {
            var session = EnhancedSimulatorAnalysisTests.CreateSession(b => b
                .WithProfessionalPlayer()
                .WithSimpleGrouping());

            var target = Target.Triple(20);
            const int throwCount = 100;

            TestContext.Out.WriteLine("=== Grouping Analysis ===\n");

            var coordinates = new List<(double X, double Y)>();
            for (int i = 0; i < throwCount; i++)
            {
                var result = session.Throw(target);
                coordinates.Add((result.HitPoint.X, result.HitPoint.Y));
            }

            // Calculate spread
            var avgX = coordinates.Average(c => c.X);
            var avgY = coordinates.Average(c => c.Y);

            var variance = coordinates.Sum(c =>
            {
                var dx = c.X - avgX;
                var dy = c.Y - avgY;
                return (dx * dx) + (dy * dy);
            }) / throwCount;

            var stdDev = Math.Sqrt(variance);

            TestContext.Out.WriteLine($"Throws: {throwCount}");
            TestContext.Out.WriteLine($"Center: ({avgX:F6}, {avgY:F6})");
            TestContext.Out.WriteLine($"Standard Deviation: {stdDev:F6}");

            Assert.That(coordinates, Has.Count.EqualTo(throwCount));
            Assert.That(stdDev, Is.GreaterThan(0));
        }

        private static Target SelectSmartTarget(int remaining)
        {
            if (remaining <= 40 && remaining % 2 == 0 && remaining >= 2)
            {
                int doubleTarget = remaining / 2;
                if (doubleTarget <= 20)
                    return Target.Double(doubleTarget);
            }

            if (remaining == 50)
                return Target.Bullseye();

            if (remaining < 60 && remaining % 2 != 0)
                return Target.Single(1);

            return Target.Triple(20);
        }
    }
}
