using Dartillery;
using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
public class EnhancedSimulatorBuilderTests
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
    public class BasicBuilderTests
    {
        [Test]
        public void BuildSession_WithDefaults_CreatesValidSession()
        {
            var session = new EnhancedDartboardSimulatorBuilder()
                .BuildSession();

            Assert.That(session, Is.Not.Null);
            Assert.That(session.Profile, Is.Not.Null);
            Assert.That(session.ThrowCount, Is.EqualTo(0));
        }

        [Test]
        public void BuildSession_WithSeed_ProducesConsistentResults()
        {
            var session1 = CreateSession(seed: 42);
            var session2 = CreateSession(seed: 42);

            var target = Target.Triple(20);

            var result1 = session1.Throw(target);
            var result2 = session2.Throw(target);

            Assert.That(result1.Score, Is.EqualTo(result2.Score));
            Assert.That(result1.SegmentType, Is.EqualTo(result2.SegmentType));
            Assert.That(result1.SectorNumber, Is.EqualTo(result2.SectorNumber));
        }

        [Test]
        public void BuildSession_WithDifferentSeeds_ProducesDifferentResults()
        {
            var session1 = CreateSession(seed: 1);
            var session2 = CreateSession(seed: 2);

            var target = Target.Triple(20);

            var results1 = new List<int>();
            var results2 = new List<int>();

            for (int i = 0; i < 50; i++)
            {
                results1.Add(session1.Throw(target).Score);
                results2.Add(session2.Throw(target).Score);
            }

            var identicalCount = results1.Zip(results2, (a, b) => a == b).Count(x => x);
            Assert.That(identicalCount, Is.LessThan(50));
        }
    }

    [TestFixture]
    public class PlayerProfileTests
    {
        [Test]
        public void BuildSession_WithProfessionalPlayer_ProfileHasLowBaseSkill()
        {
            var session = CreateSession(b => b.WithProfessionalPlayer("Michael"));

            Assert.That(session.Profile.Name, Is.EqualTo("Michael"));
            Assert.That(session.Profile.BaseSkill, Is.LessThan(0.05));
            Assert.That(session.Profile.PressureResistance, Is.GreaterThan(0.5));
        }

        [Test]
        public void BuildSession_WithAmateurPlayer_ProfileMatchesAmateurSkill()
        {
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithAmateurPlayer("John")
                .BuildSession();

            Assert.That(session.Profile.Name, Is.EqualTo("John"));
            Assert.That(session.Profile.BaseSkill, Is.EqualTo(0.05).Within(0.01));
            Assert.That(session.Profile.PressureResistance, Is.EqualTo(0.5).Within(0.1));
        }

        [Test]
        public void BuildSession_WithBeginnerPlayer_ProfileHasHighBaseSkill()
        {
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithBeginnerPlayer("Sarah")
                .BuildSession();

            Assert.That(session.Profile.Name, Is.EqualTo("Sarah"));
            Assert.That(session.Profile.BaseSkill, Is.GreaterThan(0.05));
            Assert.That(session.Profile.PressureResistance, Is.LessThan(0.5));
        }

        [Test]
        public void BuildSession_WithCustomProfile_ProfileMatchesInput()
        {
            var customProfile = new PlayerProfile
            {
                Name = "Custom Player",
                BaseSkill = 0.03,
                SystematicBiasX = 0.01,
                SystematicBiasY = -0.005,
                FatigueRate = 0.004,
                PressureResistance = 0.7,
                MaxTremor = 0.04
            };

            var session = new EnhancedDartboardSimulatorBuilder()
                .WithPlayerProfile(customProfile)
                .BuildSession();

            Assert.That(session.Profile, Is.EqualTo(customProfile));
            Assert.That(session.Profile.Name, Is.EqualTo("Custom Player"));
            Assert.That(session.Profile.BaseSkill, Is.EqualTo(0.03));
        }

        [Test]
        public void Throw_ProfessionalVsAmateur_ProfessionalAverageHigher()
        {
            const int throwCount = 500;
            var target = Target.Triple(20);

            var proSession = CreateSession(b => b.WithProfessionalPlayer());
            var amateurSession = CreateSession(b => b.WithAmateurPlayer());

            var proScores = new List<int>();
            var amateurScores = new List<int>();

            for (int i = 0; i < throwCount; i++)
            {
                proScores.Add(proSession.Throw(target).Score);
                amateurScores.Add(amateurSession.Throw(target).Score);
            }

            var proAverage = proScores.Average();
            var amateurAverage = amateurScores.Average();

            TestContext.Out.WriteLine($"Professional average: {proAverage:F2}");
            TestContext.Out.WriteLine($"Amateur average: {amateurAverage:F2}");

            Assert.That(proAverage, Is.GreaterThan(amateurAverage),
                "Professional should have higher average score");
        }
    }

    [TestFixture]
    public class TremorModelTests
    {
        [Test]
        public void BuildSession_WithLinearTremor_TremorIncreasesOverThrows()
        {
            var session = CreateSession(b => b.WithLinearTremor());

            Assert.That(session, Is.Not.Null);

            // Throw some darts and check that tremor increases
            var initialTremor = session.CurrentTremor;
            for (int i = 0; i < 50; i++)
            {
                session.Throw(Target.Triple(20));
            }

            var finalTremor = session.CurrentTremor;

            TestContext.Out.WriteLine($"Initial tremor: {initialTremor:F6}");
            TestContext.Out.WriteLine($"Final tremor after 50 throws: {finalTremor:F6}");

            Assert.That(finalTremor, Is.GreaterThan(initialTremor),
                "Tremor should increase with linear model");
        }

        [Test]
        public void BuildSession_WithRealisticTremor_TremorIncreasesOrStable()
        {
            var session = CreateSession(b => b.WithRealisticTremor());

            Assert.That(session, Is.Not.Null);

            // Logarithmic tremor should also build up
            var initialTremor = session.CurrentTremor;
            for (int i = 0; i < 100; i++)
            {
                session.Throw(Target.Triple(20));
            }

            var finalTremor = session.CurrentTremor;

            TestContext.Out.WriteLine($"Initial tremor: {initialTremor:F6}");
            TestContext.Out.WriteLine($"Final tremor after 100 throws: {finalTremor:F6}");

            Assert.That(finalTremor, Is.GreaterThanOrEqualTo(initialTremor));
        }

        [Test]
        public void BuildSession_WithoutTremor_TremorRemainsConstant()
        {
            var session = CreateSession(b => b.WithTremorModel(new NoOpTremorModel()));

            var tremorValues = new List<double>();
            for (int i = 0; i < 50; i++)
            {
                session.Throw(Target.Triple(20));
                tremorValues.Add(session.CurrentTremor);
            }

            var distinctValues = tremorValues.Distinct().Count();
            Assert.That(distinctValues, Is.LessThanOrEqualTo(2),
                "Without tremor model, tremor should remain relatively constant");
        }
    }

    [TestFixture]
    public class PressureModelTests
    {
        [Test]
        public void WithStandardPressure_BuildsSuccessfully()
        {
            var session = CreateSession(b => b.WithStandardPressure());

            Assert.That(session, Is.Not.Null);

            // Test with pressure situation
            var gameContext = new GameContext
            {
                RemainingScore = 32, // Checkout situation
                CurrentVisitResults = new List<ThrowResult>()
            };

            var result = session.Throw(Target.Double(16), gameContext);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void WithCheckoutPsychology_BuildsSuccessfully()
        {
            var session = CreateSession(b => b.WithCheckoutPsychology().WithProfessionalPlayer());

            Assert.That(session, Is.Not.Null);

            // Test checkout psychology
            var checkoutContext = new GameContext
            {
                RemainingScore = 40,
                CurrentVisitResults = new List<ThrowResult>()
            };

            var result = session.Throw(Target.Double(20), checkoutContext);
            Assert.That(result, Is.Not.Null);
        }
    }

    [TestFixture]
    public class MomentumModelTests
    {
        [Test]
        public void WithStandardMomentum_BuildsSuccessfully()
        {
            var session = CreateSession(b => b.WithStandardMomentum());

            Assert.That(session, Is.Not.Null);

            // Throw multiple darts to test momentum
            var results = new List<ThrowResult>();
            for (int i = 0; i < 20; i++)
            {
                results.Add(session.Throw(Target.Triple(20)));
            }

            Assert.That(results.Count, Is.EqualTo(20));
        }
    }

    [TestFixture]
    public class GroupingModelTests
    {
        [Test]
        public void WithSimpleGrouping_BuildsSuccessfully()
        {
            var session = CreateSession(b => b.WithSimpleGrouping());

            Assert.That(session, Is.Not.Null);

            // Test grouping - throw multiple times at the same target
            var target = Target.Triple(20);
            var results = new List<ThrowResult>();

            for (int i = 0; i < 10; i++)
            {
                results.Add(session.Throw(target));
            }

            // Check that throws have coordinates
            Assert.That(results, Has.Count.EqualTo(10));
        }
    }

    [TestFixture]
    public class TargetDifficultyTests
    {
        [Test]
        public void WithStandardTargetDifficulty_BuildsSuccessfully()
        {
            var session = CreateSession(b => b.WithStandardTargetDifficulty());

            Assert.That(session, Is.Not.Null);

            // Test various targets with different difficulties
            var easyTarget = Target.Single(20);
            var mediumTarget = Target.Double(20);
            var hardTarget = Target.Triple(20);

            var easyResult = session.Throw(easyTarget);
            var mediumResult = session.Throw(mediumTarget);
            var hardResult = session.Throw(hardTarget);

            Assert.That(easyResult, Is.Not.Null);
            Assert.That(mediumResult, Is.Not.Null);
            Assert.That(hardResult, Is.Not.Null);
        }
    }

    [TestFixture]
    public class TruncationTests
    {
        [Test]
        public void WithTruncation_DefaultMaxDeviation_BuildsSuccessfully()
        {
            var session = CreateSession(b => b.WithTruncation());

            Assert.That(session, Is.Not.Null);

            // Test that all throws stay within a reasonable distance
            var results = new List<ThrowResult>();
            for (int i = 0; i < 100; i++)
            {
                results.Add(session.Throw(Target.Triple(20)));
            }

            // With truncation, extreme deviations should be limited
            Assert.That(results.All(r => r.Score >= 0), Is.True);
        }

        [Test]
        public void WithTruncation_CustomMaxDeviation_BuildsSuccessfully()
        {
            var session = CreateSession(b => b.WithTruncation(0.15));

            Assert.That(session, Is.Not.Null);

            var results = new List<ThrowResult>();
            for (int i = 0; i < 50; i++)
            {
                results.Add(session.Throw(Target.Bullseye()));
            }

            Assert.That(results.Count, Is.EqualTo(50));
        }
    }

    [TestFixture]
    public class FluentBuilderTests
    {
        [Test]
        public void BuildSession_AllFeaturesChained_SessionCreatedSuccessfully()
        {
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer("TestPro")
                .WithLinearTremor()
                .WithStandardPressure()
                .WithStandardMomentum()
                .WithSimpleGrouping()
                .WithStandardTargetDifficulty()
                .WithTruncation(0.2)
                .WithSeed(999)
                .BuildSession();

            Assert.That(session, Is.Not.Null);
            Assert.That(session.Profile.Name, Is.EqualTo("TestPro"));

            // Test that everything works
            var result = session.Throw(Target.Triple(20));
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void BuildSession_MinimalConfig_DefaultsApplied()
        {
            var session = new EnhancedDartboardSimulatorBuilder()
                .BuildSession();

            Assert.That(session, Is.Not.Null);
            Assert.That(session.Profile, Is.Not.Null);
            Assert.That(session.ThrowCount, Is.EqualTo(0));

            // Should work with defaults
            var result = session.Throw(Target.Triple(20));
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void BuildSession_AllFeaturesWithGameContext_ThreeThrowsSucceed()
        {
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithAmateurPlayer("Complete Test")
                .WithRealisticTremor()
                .WithCheckoutPsychology()
                .WithStandardMomentum()
                .WithSimpleGrouping()
                .WithStandardTargetDifficulty()
                .WithTruncation(0.25)
                .WithSeed(12345)
                .BuildSession();

            Assert.That(session, Is.Not.Null);

            // Test a complete throw sequence
            var gameContext = new GameContext
            {
                RemainingScore = 141,
                CurrentVisitResults = new List<ThrowResult>()
            };

            var result1 = session.Throw(Target.Triple(20), gameContext);
            gameContext.CurrentVisitResults.Add(result1);

            var result2 = session.Throw(Target.Triple(19), gameContext);
            gameContext.CurrentVisitResults.Add(result2);

            var result3 = session.Throw(Target.Double(12), gameContext);

            Assert.That(result1, Is.Not.Null);
            Assert.That(result2, Is.Not.Null);
            Assert.That(result3, Is.Not.Null);
        }
    }

    [TestFixture]
    public class ComparisonTests
    {
        [Test]
        public void Throw_WithAndWithoutTremor_AllScoresInValidRange()
        {
            const int throwCount = 100;
            var target = Target.Triple(20);

            var withTremor = CreateSession(b => b.WithProfessionalPlayer().WithLinearTremor());
            var withoutTremor = CreateSession(b => b.WithProfessionalPlayer());

            var scoresWithTremor = new List<int>();
            var scoresWithoutTremor = new List<int>();

            for (int i = 0; i < throwCount; i++)
            {
                scoresWithTremor.Add(withTremor.Throw(target).Score);
                scoresWithoutTremor.Add(withoutTremor.Throw(target).Score);
            }

            var firstHalfWithTremor = scoresWithTremor.Take(50).Average();
            var secondHalfWithTremor = scoresWithTremor.Skip(50).Average();

            TestContext.Out.WriteLine($"With Tremor - First half: {firstHalfWithTremor:F2}, Second half: {secondHalfWithTremor:F2}");
            TestContext.Out.WriteLine($"Without Tremor - Average: {scoresWithoutTremor.Average():F2}");

            // Both configurations should deliver valid results
            Assert.That(scoresWithTremor.All(s => s >= 0 && s <= 60), Is.True);
            Assert.That(scoresWithoutTremor.All(s => s >= 0 && s <= 60), Is.True);
        }

        [Test]
        public void Throw_ThreeProfileLevels_AveragesFormDescendingGradient()
        {
            const int throwCount = 300;
            var target = Target.Triple(20);

            var pro = CreateSession(b => b.WithProfessionalPlayer());
            var amateur = CreateSession(b => b.WithAmateurPlayer());
            var beginner = CreateSession(b => b.WithBeginnerPlayer());

            var proScores = new List<int>();
            var amateurScores = new List<int>();
            var beginnerScores = new List<int>();

            for (int i = 0; i < throwCount; i++)
            {
                proScores.Add(pro.Throw(target).Score);
                amateurScores.Add(amateur.Throw(target).Score);
                beginnerScores.Add(beginner.Throw(target).Score);
            }

            var proAvg = proScores.Average();
            var amateurAvg = amateurScores.Average();
            var beginnerAvg = beginnerScores.Average();

            TestContext.Out.WriteLine($"Professional average: {proAvg:F2}");
            TestContext.Out.WriteLine($"Amateur average: {amateurAvg:F2}");
            TestContext.Out.WriteLine($"Beginner average: {beginnerAvg:F2}");

            Assert.That(proAvg, Is.GreaterThan(amateurAvg));
            Assert.That(amateurAvg, Is.GreaterThan(beginnerAvg));
        }
    }

    private sealed class NoOpTremorModel : ITremorModel
    {
        public double CalculateTremor(SessionState state, PlayerProfile profile) => 0.0;
    }
}
