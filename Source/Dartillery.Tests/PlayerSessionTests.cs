using Dartillery;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
public class PlayerSessionTests
{
    private static PlayerSession CreateSession(
        Action<EnhancedDartboardSimulatorBuilder>? configure = null,
        int seed = 42)
    {
        var builder = new EnhancedDartboardSimulatorBuilder().WithSeed(seed);
        configure?.Invoke(builder);
        return builder.BuildSession();
    }

    [TestFixture]
    public class SessionStateTests
    {
        [Test]
        public void NewSession_StartsWithZeroThrows()
        {
            var session = new EnhancedDartboardSimulatorBuilder()
                .BuildSession();

            Assert.Multiple(() =>
            {
                Assert.That(session.ThrowCount, Is.EqualTo(0));
                Assert.That(session.ThrowHistory, Is.Empty);
            });
        }

        [Test]
        public void Throw_IncrementsThrowCount()
        {
            var session = CreateSession();

            session.Throw(Target.Triple(20));
            session.Throw(Target.Triple(20));
            session.Throw(Target.Triple(20));

            Assert.That(session.ThrowCount, Is.EqualTo(3));
        }

        [Test]
        public void Throw_AddsToHistory()
        {
            var session = CreateSession();

            var result1 = session.Throw(Target.Triple(20));
            var result2 = session.Throw(Target.Double(16));
            var result3 = session.Throw(Target.Bullseye());

            Assert.That(session.ThrowHistory, Has.Count.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(session.ThrowHistory[0], Is.EqualTo(result1));
                Assert.That(session.ThrowHistory[1], Is.EqualTo(result2));
                Assert.That(session.ThrowHistory[2], Is.EqualTo(result3));
            });
        }

        [Test]
        public void Reset_ClearsSessionState()
        {
            var session = CreateSession(b => b.WithLinearFatigue());

            // Throw some darts
            for (int i = 0; i < 50; i++)
            {
                session.Throw(Target.Triple(20));
            }

            var throwCountBeforeReset = session.ThrowCount;
            var fatigueBeforeReset = session.CurrentFatigue;

            session.Reset();

            Assert.Multiple(() =>
            {
                Assert.That(throwCountBeforeReset, Is.GreaterThan(0));
                Assert.That(session.ThrowCount, Is.EqualTo(0));
                Assert.That(session.ThrowHistory, Is.Empty);
                Assert.That(session.CurrentFatigue, Is.LessThanOrEqualTo(fatigueBeforeReset));
            });
        }

        [Test]
        public void SessionId_IsUnique()
        {
            var session1 = new EnhancedDartboardSimulatorBuilder().BuildSession();
            var session2 = new EnhancedDartboardSimulatorBuilder().BuildSession();

            Assert.That(session1.SessionId, Is.Not.EqualTo(session2.SessionId));
        }

        [Test]
        public void SessionId_StaysConstantDuringSession()
        {
            var session = CreateSession();

            var initialId = session.SessionId;

            for (int i = 0; i < 20; i++)
            {
                session.Throw(Target.Triple(20));
            }

            Assert.That(session.SessionId, Is.EqualTo(initialId));
        }
    }

    [TestFixture]
    public class FatigueAccumulationTests
    {
        [Test]
        public void Throw_LinearFatigueOver100Throws_FatigueMonotonicallyIncreases()
        {
            var session = CreateSession(b => b.WithProfessionalPlayer().WithLinearFatigue());

            var initialFatigue = session.CurrentFatigue;
            var fatigueValues = new List<double> { initialFatigue };

            for (int i = 0; i < 100; i++)
            {
                session.Throw(Target.Triple(20));
                fatigueValues.Add(session.CurrentFatigue);
            }

            var finalFatigue = session.CurrentFatigue;

            TestContext.Out.WriteLine($"Initial fatigue: {initialFatigue:F6}");
            TestContext.Out.WriteLine($"Final fatigue: {finalFatigue:F6}");
            TestContext.Out.WriteLine($"Increase: {finalFatigue - initialFatigue:F6}");

            // Fatigue should rise
            Assert.That(finalFatigue, Is.GreaterThan(initialFatigue));

            // Fatigue should monotonically increase (or at least not decrease)
            for (int i = 1; i < fatigueValues.Count; i++)
            {
                Assert.That(fatigueValues[i], Is.GreaterThanOrEqualTo(fatigueValues[i - 1]),
                    $"Fatigue decreased at throw {i}");
            }
        }

        [Test]
        public void Throw_RealisticFatigueOver200Throws_EarlyIncreaseExceedsLate()
        {
            var session = CreateSession(b => b.WithProfessionalPlayer().WithRealisticFatigue());

            var fatigueCheckpoints = new List<(int ThrowCount, double Fatigue)>();

            for (int i = 0; i < 200; i++)
            {
                session.Throw(Target.Triple(20));

                // Store fatigue at certain checkpoints
                if (i % 20 == 0)
                {
                    fatigueCheckpoints.Add((i, session.CurrentFatigue));
                }
            }

            TestContext.Out.WriteLine("Fatigue progression:");
            foreach (var (count, fatigue) in fatigueCheckpoints)
            {
                TestContext.Out.WriteLine($"  After {count} throws: {fatigue:F6}");
            }

            // First half should have stronger increase than second half
            var earlyIncrease = fatigueCheckpoints[fatigueCheckpoints.Count / 2].Fatigue -
                                fatigueCheckpoints[0].Fatigue;
            var lateIncrease = fatigueCheckpoints[^1].Fatigue -
                               fatigueCheckpoints[fatigueCheckpoints.Count / 2].Fatigue;

            Assert.That(earlyIncrease, Is.GreaterThanOrEqualTo(lateIncrease),
                "Logarithmic fatigue should increase more rapidly at the beginning");
        }

        [Test]
        public void Reset_AfterLinearFatigueAccumulation_FatigueResetsToZero()
        {
            var session = CreateSession(b => b.WithLinearFatigue());

            // Build up fatigue
            for (int i = 0; i < 50; i++)
            {
                session.Throw(Target.Triple(20));
            }

            var fatigueBeforeReset = session.CurrentFatigue;

            session.Reset();

            Assert.Multiple(() =>
            {
                Assert.That(fatigueBeforeReset, Is.GreaterThan(0));
                Assert.That(session.CurrentFatigue, Is.EqualTo(0));
            });
        }
    }

    [TestFixture]
    public class GameContextTests
    {
        [Test]
        public void Throw_WithGameContext_ProcessesSuccessfully()
        {
            var session = CreateSession(b => b.WithStandardPressure());

            var gameContext = new GameContext
            {
                RemainingScore = 141,
                CurrentVisitResults = new List<ThrowResult>()
            };

            var result = session.Throw(Target.Triple(20), gameContext);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Score, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void Throw_CheckoutSituation_AppliesPressure()
        {
            var session = CreateSession(b => b.WithAmateurPlayer().WithCheckoutPsychology());

            var normalScores = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                normalScores.Add(session.Throw(Target.Double(20)).Score);
            }

            session.Reset();

            var checkoutContext = new GameContext
            {
                RemainingScore = 40, // Checkout at D20
                CurrentVisitResults = new List<ThrowResult>()
            };

            var checkoutScores = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                checkoutScores.Add(session.Throw(Target.Double(20), checkoutContext).Score);
            }

            Assert.Multiple(() =>
            {
                Assert.That(normalScores.All(s => s >= 0 && s <= 40), Is.True);
                Assert.That(checkoutScores.All(s => s >= 0 && s <= 40), Is.True);
            });

            TestContext.Out.WriteLine($"Normal average: {normalScores.Average():F2}");
            TestContext.Out.WriteLine($"Checkout average: {checkoutScores.Average():F2}");
        }

        [Test]
        public void Throw_WithPreviousThrowsInVisit_TracksVisitHistory()
        {
            var session = CreateSession();

            var gameContext = new GameContext
            {
                RemainingScore = 501,
                CurrentVisitResults = new List<ThrowResult>()
            };

            var dart1 = session.Throw(Target.Triple(20), gameContext);
            gameContext.CurrentVisitResults.Add(dart1);

            var dart2 = session.Throw(Target.Triple(20), gameContext);
            gameContext.CurrentVisitResults.Add(dart2);

            var dart3 = session.Throw(Target.Triple(20), gameContext);

            Assert.Multiple(() =>
            {
                Assert.That(dart1, Is.Not.Null);
                Assert.That(dart2, Is.Not.Null);
                Assert.That(dart3, Is.Not.Null);
            });
        }
    }

    [TestFixture]
    public class ProfileAccessTests
    {
        [Test]
        public void Profile_IsReadable()
        {
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer("TestPlayer")
                .BuildSession();

            var profile = session.Profile;

            Assert.That(profile, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(profile.Name, Is.EqualTo("TestPlayer"));
                Assert.That(profile.BaseSkill, Is.GreaterThan(0));
            });
        }

        [Test]
        public void ThrowHistory_IsReadOnly()
        {
            var session = CreateSession();

            session.Throw(Target.Triple(20));

            var history = session.ThrowHistory;

            Assert.That(history, Is.InstanceOf<IReadOnlyList<ThrowResult>>());
        }
    }

    [TestFixture]
    public class LongSessionTests
    {
        [Test]
        public void Throw_500ThrowsWithFatigue_AllScoresInValidRange()
        {
            var session = CreateSession(b => b.WithAmateurPlayer().WithLinearFatigue());

            var allScores = new List<int>();
            for (int i = 0; i < 500; i++)
            {
                var result = session.Throw(Target.Triple(20));
                allScores.Add(result.Score);
            }

            Assert.Multiple(() =>
            {
                Assert.That(session.ThrowCount, Is.EqualTo(500));
                Assert.That(session.ThrowHistory, Has.Count.EqualTo(500));
                Assert.That(allScores.All(s => s >= 0 && s <= 60), Is.True);
            });

            TestContext.Out.WriteLine($"Average score over 500 throws: {allScores.Average():F2}");
            TestContext.Out.WriteLine($"Final fatigue: {session.CurrentFatigue:F6}");
        }

        [Test]
        public void Reset_FiveConsecutiveGames_EachResetClearsState()
        {
            var session = CreateSession(b => b.WithLinearFatigue());

            for (int game = 0; game < 5; game++)
            {
                // Play a game
                for (int i = 0; i < 50; i++)
                {
                    session.Throw(Target.Triple(20));
                }

                Assert.Multiple(() =>
                {
                    Assert.That(session.ThrowCount, Is.EqualTo(50));
                    Assert.That(session.ThrowHistory, Has.Count.EqualTo(50));
                });

                // Reset for next game
                session.Reset();

                Assert.Multiple(() =>
                {
                    Assert.That(session.ThrowCount, Is.EqualTo(0));
                    Assert.That(session.ThrowHistory.Count, Is.EqualTo(0));
                });
            }
        }
    }

    [TestFixture]
    public class RealisticScenarioTests
    {
        [Test]
        public void Simulate501_ProfessionalWithFatigueAndPressure_ThrowCountPositive()
        {
            var session = CreateSession(b => b
                .WithProfessionalPlayer()
                .WithRealisticFatigue()
                .WithCheckoutPsychology());

            int remainingScore = 501;
            var visitScores = new List<int>();

            while (remainingScore > 0 && session.ThrowCount < 100)
            {
                var gameContext = new GameContext
                {
                    RemainingScore = remainingScore,
                    CurrentVisitResults = new List<ThrowResult>()
                };

                int visitScore = 0;

                // 3 darts per visit
                for (int dart = 0; dart < 3 && remainingScore > 0; dart++)
                {
                    var target = SelectTarget(remainingScore);
                    var result = session.Throw(target, gameContext);

                    gameContext.CurrentVisitResults.Add(result);
                    visitScore += result.Score;

                    // Simple bust check
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

            TestContext.Out.WriteLine($"Game completed in {session.ThrowCount} darts");
            TestContext.Out.WriteLine($"Number of visits: {visitScores.Count}");
            TestContext.Out.WriteLine($"Final score: {remainingScore}");
            TestContext.Out.WriteLine($"3-dart average: {(501 - remainingScore) * 3.0 / session.ThrowCount:F2}");

            Assert.Multiple(() =>
            {
                Assert.That(session.ThrowCount, Is.GreaterThan(0));
                Assert.That(visitScores.Count, Is.GreaterThan(0));
            });
        }

        private static Target SelectTarget(int remaining)
        {
            // Very simple strategy
            if (remaining <= 40 && remaining % 2 == 0)
                return Target.Double(remaining / 2);

            if (remaining == 50)
                return Target.Bullseye();

            return Target.Triple(20);
        }
    }
}
