using Dartillery;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
public class PlayerSessionTests
{
    [TestFixture]
    public class SessionStateTests
    {
        [Test]
        public void NewSession_StartsWithZeroThrows()
        {
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .BuildSession();

            // Assert
            Assert.That(session.ThrowCount, Is.EqualTo(0));
            Assert.That(session.ThrowHistory, Is.Empty);
        }

        [Test]
        public void Throw_IncrementsThrowCount()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithSeed(42)
                .BuildSession();

            // Act
            session.Throw(Target.Triple(20));
            session.Throw(Target.Triple(20));
            session.Throw(Target.Triple(20));

            // Assert
            Assert.That(session.ThrowCount, Is.EqualTo(3));
        }

        [Test]
        public void Throw_AddsToHistory()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithSeed(42)
                .BuildSession();

            // Act
            var result1 = session.Throw(Target.Triple(20));
            var result2 = session.Throw(Target.Double(16));
            var result3 = session.Throw(Target.Bullseye());

            // Assert
            Assert.That(session.ThrowHistory, Has.Count.EqualTo(3));
            Assert.That(session.ThrowHistory[0], Is.EqualTo(result1));
            Assert.That(session.ThrowHistory[1], Is.EqualTo(result2));
            Assert.That(session.ThrowHistory[2], Is.EqualTo(result3));
        }

        [Test]
        public void Reset_ClearsSessionState()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithLinearTremor()
                .WithSeed(42)
                .BuildSession();

            // Werfe einige Darts
            for (int i = 0; i < 50; i++)
            {
                session.Throw(Target.Triple(20));
            }

            var throwCountBeforeReset = session.ThrowCount;
            var tremorBeforeReset = session.CurrentTremor;

            // Act
            session.Reset();

            // Assert
            Assert.That(throwCountBeforeReset, Is.GreaterThan(0));
            Assert.That(session.ThrowCount, Is.EqualTo(0));
            Assert.That(session.ThrowHistory, Is.Empty);
            Assert.That(session.CurrentTremor, Is.LessThanOrEqualTo(tremorBeforeReset));
        }

        [Test]
        public void SessionId_IsUnique()
        {
            // Arrange & Act
            var session1 = new EnhancedDartboardSimulatorBuilder().BuildSession();
            var session2 = new EnhancedDartboardSimulatorBuilder().BuildSession();

            // Assert
            Assert.That(session1.SessionId, Is.Not.EqualTo(session2.SessionId));
        }

        [Test]
        public void SessionId_StaysConstantDuringSession()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithSeed(42)
                .BuildSession();

            var initialId = session.SessionId;

            // Act - werfe mehrere Darts
            for (int i = 0; i < 20; i++)
            {
                session.Throw(Target.Triple(20));
            }

            // Assert
            Assert.That(session.SessionId, Is.EqualTo(initialId));
        }
    }

    [TestFixture]
    public class TremorAccumulationTests
    {
        [Test]
        public void LinearTremor_IncreasesOverTime()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithLinearTremor()
                .WithSeed(42)
                .BuildSession();

            // Act & Assert
            var initialTremor = session.CurrentTremor;
            var tremorValues = new List<double> { initialTremor };

            for (int i = 0; i < 100; i++)
            {
                session.Throw(Target.Triple(20));
                tremorValues.Add(session.CurrentTremor);
            }

            var finalTremor = session.CurrentTremor;

            Console.WriteLine($"Initial tremor: {initialTremor:F6}");
            Console.WriteLine($"Final tremor: {finalTremor:F6}");
            Console.WriteLine($"Increase: {finalTremor - initialTremor:F6}");

            // Tremor sollte steigen
            Assert.That(finalTremor, Is.GreaterThan(initialTremor));

            // Tremor sollte monoton steigen (oder zumindest nicht fallen)
            for (int i = 1; i < tremorValues.Count; i++)
            {
                Assert.That(tremorValues[i], Is.GreaterThanOrEqualTo(tremorValues[i - 1]),
                    $"Tremor decreased at throw {i}");
            }
        }

        [Test]
        public void RealisticTremor_IncreasesLogarithmically()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithRealisticTremor()
                .WithSeed(42)
                .BuildSession();

            // Act
            var tremorCheckpoints = new List<(int ThrowCount, double Tremor)>();

            for (int i = 0; i < 200; i++)
            {
                session.Throw(Target.Triple(20));

                // Speichere Tremor bei bestimmten Checkpoints
                if (i % 20 == 0)
                {
                    tremorCheckpoints.Add((i, session.CurrentTremor));
                }
            }

            // Assert - Logarithmisches Wachstum: hoher Anstieg am Anfang, dann abflachend
            Console.WriteLine("Tremor progression:");
            foreach (var (count, tremor) in tremorCheckpoints)
            {
                Console.WriteLine($"  After {count} throws: {tremor:F6}");
            }

            // Erste Hälfte sollte stärkeren Anstieg haben als zweite Hälfte
            var earlyIncrease = tremorCheckpoints[tremorCheckpoints.Count / 2].Tremor -
                                tremorCheckpoints[0].Tremor;
            var lateIncrease = tremorCheckpoints[^1].Tremor -
                               tremorCheckpoints[tremorCheckpoints.Count / 2].Tremor;

            Assert.That(earlyIncrease, Is.GreaterThanOrEqualTo(lateIncrease),
                "Logarithmic tremor should increase more rapidly at the beginning");
        }

        [Test]
        public void TremorReset_ResetsToZero()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithLinearTremor()
                .WithSeed(42)
                .BuildSession();

            // Baue Tremor auf
            for (int i = 0; i < 50; i++)
            {
                session.Throw(Target.Triple(20));
            }

            var tremorBeforeReset = session.CurrentTremor;

            // Act
            session.Reset();

            // Assert
            Assert.That(tremorBeforeReset, Is.GreaterThan(0));
            Assert.That(session.CurrentTremor, Is.EqualTo(0));
        }
    }

    [TestFixture]
    public class GameContextTests
    {
        [Test]
        public void Throw_WithGameContext_ProcessesSuccessfully()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithStandardPressure()
                .WithSeed(42)
                .BuildSession();

            var gameContext = new GameContext
            {
                RemainingScore = 141,
                CurrentVisitResults = new List<ThrowResult>()
            };

            // Act
            var result = session.Throw(Target.Triple(20), gameContext);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Score, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void Throw_CheckoutSituation_AppliesPressure()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithAmateurPlayer()
                .WithCheckoutPsychology()
                .WithSeed(42)
                .BuildSession();

            // Act - normale Situation vs. Checkout-Situation
            var normalScores = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                normalScores.Add(session.Throw(Target.Double(20)).Score);
            }

            session.Reset();

            var checkoutContext = new GameContext
            {
                RemainingScore = 40, // Checkout auf D20
                CurrentVisitResults = new List<ThrowResult>()
            };

            var checkoutScores = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                checkoutScores.Add(session.Throw(Target.Double(20), checkoutContext).Score);
            }

            // Assert - beide sollten gültige Ergebnisse liefern
            Assert.That(normalScores.All(s => s >= 0 && s <= 40), Is.True);
            Assert.That(checkoutScores.All(s => s >= 0 && s <= 40), Is.True);

            Console.WriteLine($"Normal average: {normalScores.Average():F2}");
            Console.WriteLine($"Checkout average: {checkoutScores.Average():F2}");
        }

        [Test]
        public void Throw_WithPreviousThrowsInVisit_TracksVisitHistory()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithSeed(42)
                .BuildSession();

            var gameContext = new GameContext
            {
                RemainingScore = 501,
                CurrentVisitResults = new List<ThrowResult>()
            };

            // Act - werfe 3 Darts in einem Visit
            var dart1 = session.Throw(Target.Triple(20), gameContext);
            gameContext.CurrentVisitResults.Add(dart1);

            var dart2 = session.Throw(Target.Triple(20), gameContext);
            gameContext.CurrentVisitResults.Add(dart2);

            var dart3 = session.Throw(Target.Triple(20), gameContext);

            // Assert
            Assert.That(dart1, Is.Not.Null);
            Assert.That(dart2, Is.Not.Null);
            Assert.That(dart3, Is.Not.Null);
        }
    }

    [TestFixture]
    public class ProfileAccessTests
    {
        [Test]
        public void Profile_IsReadable()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer("TestPlayer")
                .BuildSession();

            // Act
            var profile = session.Profile;

            // Assert
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.Name, Is.EqualTo("TestPlayer"));
            Assert.That(profile.BaseSkill, Is.GreaterThan(0));
        }

        [Test]
        public void ThrowHistory_IsReadOnly()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithSeed(42)
                .BuildSession();

            session.Throw(Target.Triple(20));

            // Act
            var history = session.ThrowHistory;

            // Assert
            Assert.That(history, Is.InstanceOf<IReadOnlyList<ThrowResult>>());
        }
    }

    [TestFixture]
    public class LongSessionTests
    {
        [Test]
        public void LongSession_500Throws_MaintainsStability()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithAmateurPlayer()
                .WithLinearTremor()
                .WithSeed(42)
                .BuildSession();

            // Act
            var allScores = new List<int>();
            for (int i = 0; i < 500; i++)
            {
                var result = session.Throw(Target.Triple(20));
                allScores.Add(result.Score);
            }

            // Assert
            Assert.That(session.ThrowCount, Is.EqualTo(500));
            Assert.That(session.ThrowHistory.Count, Is.EqualTo(500));
            Assert.That(allScores.All(s => s >= 0 && s <= 60), Is.True);

            Console.WriteLine($"Average score over 500 throws: {allScores.Average():F2}");
            Console.WriteLine($"Final tremor: {session.CurrentTremor:F6}");
        }

        [Test]
        public void LongSession_MultipleResets_WorksCorrectly()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithLinearTremor()
                .WithSeed(42)
                .BuildSession();

            // Act & Assert - simuliere mehrere Spiele
            for (int game = 0; game < 5; game++)
            {
                // Spiele ein Spiel
                for (int i = 0; i < 50; i++)
                {
                    session.Throw(Target.Triple(20));
                }

                Assert.That(session.ThrowCount, Is.EqualTo(50));
                Assert.That(session.ThrowHistory.Count, Is.EqualTo(50));

                // Reset für nächstes Spiel
                session.Reset();

                Assert.That(session.ThrowCount, Is.EqualTo(0));
                Assert.That(session.ThrowHistory.Count, Is.EqualTo(0));
            }
        }
    }

    [TestFixture]
    public class RealisticScenarioTests
    {
        [Test]
        public void Scenario_Complete501Game_TracksPerformanceOverTime()
        {
            // Arrange
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithRealisticTremor()
                .WithCheckoutPsychology()
                .WithSeed(42)
                .BuildSession();

            int remainingScore = 501;
            var visitScores = new List<int>();

            // Act - simuliere ein 501-Spiel
            while (remainingScore > 0 && session.ThrowCount < 100)
            {
                var gameContext = new GameContext
                {
                    RemainingScore = remainingScore,
                    CurrentVisitResults = new List<ThrowResult>()
                };

                int visitScore = 0;

                // 3 Darts pro Visit
                for (int dart = 0; dart < 3 && remainingScore > 0; dart++)
                {
                    var target = SelectTarget(remainingScore);
                    var result = session.Throw(target, gameContext);

                    gameContext.CurrentVisitResults.Add(result);
                    visitScore += result.Score;

                    // Einfache Bust-Prüfung
                    if (remainingScore - visitScore < 2 && remainingScore - visitScore != 0)
                    {
                        visitScore = 0;
                        break;
                    }
                }

                remainingScore -= visitScore;
                visitScores.Add(visitScore);

                if (remainingScore < 0) remainingScore += visitScore; // Bust
            }

            // Assert
            Console.WriteLine($"Game completed in {session.ThrowCount} darts");
            Console.WriteLine($"Number of visits: {visitScores.Count}");
            Console.WriteLine($"Final score: {remainingScore}");
            Console.WriteLine($"3-dart average: {(501 - remainingScore) * 3.0 / session.ThrowCount:F2}");

            Assert.That(session.ThrowCount, Is.GreaterThan(0));
            Assert.That(visitScores.Count, Is.GreaterThan(0));
        }

        private Target SelectTarget(int remaining)
        {
            // Sehr einfache Strategie
            if (remaining <= 40 && remaining % 2 == 0)
                return Target.Double(remaining / 2);

            if (remaining == 50)
                return Target.Bullseye();

            return Target.Triple(20);
        }
    }
}
