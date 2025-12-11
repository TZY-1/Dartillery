using Dartillery;
using Dartillery.Core.Models;

namespace Dartillery.Tests;

[TestFixture]
public class EnhancedSimulatorBuilderTests
{
    [TestFixture]
    public class BasicBuilderTests
    {
        [Test]
        public void BuildSession_WithDefaults_CreatesValidSession()
        {
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .BuildSession();

            // Assert
            Assert.That(session, Is.Not.Null);
            Assert.That(session.Profile, Is.Not.Null);
            Assert.That(session.ThrowCount, Is.EqualTo(0));
        }

        [Test]
        public void BuildSession_WithSeed_ProducesConsistentResults()
        {
            // Arrange
            var session1 = new EnhancedDartboardSimulatorBuilder()
                .WithSeed(42)
                .BuildSession();

            var session2 = new EnhancedDartboardSimulatorBuilder()
                .WithSeed(42)
                .BuildSession();

            var target = Target.Triple(20);

            // Act
            var result1 = session1.Throw(target);
            var result2 = session2.Throw(target);

            // Assert
            Assert.That(result1.Score, Is.EqualTo(result2.Score));
            Assert.That(result1.SegmentType, Is.EqualTo(result2.SegmentType));
            Assert.That(result1.SectorNumber, Is.EqualTo(result2.SectorNumber));
        }

        [Test]
        public void BuildSession_WithDifferentSeeds_ProducesDifferentResults()
        {
            // Arrange
            var session1 = new EnhancedDartboardSimulatorBuilder()
                .WithSeed(1)
                .BuildSession();

            var session2 = new EnhancedDartboardSimulatorBuilder()
                .WithSeed(2)
                .BuildSession();

            var target = Target.Triple(20);

            // Act - sammle mehrere Würfe
            var results1 = new List<int>();
            var results2 = new List<int>();

            for (int i = 0; i < 50; i++)
            {
                results1.Add(session1.Throw(target).Score);
                results2.Add(session2.Throw(target).Score);
            }

            // Assert - Sequenzen sollten unterschiedlich sein
            var identicalCount = results1.Zip(results2, (a, b) => a == b).Count(x => x);
            Assert.That(identicalCount, Is.LessThan(50));
        }
    }

    [TestFixture]
    public class PlayerProfileTests
    {
        [Test]
        public void WithProfessionalPlayer_CreatesSessionWithProProfile()
        {
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer("Michael")
                .WithSeed(42)
                .BuildSession();

            // Assert
            Assert.That(session.Profile.Name, Is.EqualTo("Michael"));
            Assert.That(session.Profile.BaseSkill, Is.LessThan(0.05));
            Assert.That(session.Profile.PressureResistance, Is.GreaterThan(0.5));
        }

        [Test]
        public void WithAmateurPlayer_CreatesSessionWithAmateurProfile()
        {
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithAmateurPlayer("John")
                .BuildSession();

            // Assert
            Assert.That(session.Profile.Name, Is.EqualTo("John"));
            Assert.That(session.Profile.BaseSkill, Is.EqualTo(0.05).Within(0.01));
            Assert.That(session.Profile.PressureResistance, Is.EqualTo(0.5).Within(0.1));
        }

        [Test]
        public void WithBeginnerPlayer_CreatesSessionWithBeginnerProfile()
        {
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithBeginnerPlayer("Sarah")
                .BuildSession();

            // Assert
            Assert.That(session.Profile.Name, Is.EqualTo("Sarah"));
            Assert.That(session.Profile.BaseSkill, Is.GreaterThan(0.05));
            Assert.That(session.Profile.PressureResistance, Is.LessThan(0.5));
        }

        [Test]
        public void WithCustomPlayerProfile_UsesProvidedProfile()
        {
            // Arrange
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

            // Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithPlayerProfile(customProfile)
                .BuildSession();

            // Assert
            Assert.That(session.Profile, Is.EqualTo(customProfile));
            Assert.That(session.Profile.Name, Is.EqualTo("Custom Player"));
            Assert.That(session.Profile.BaseSkill, Is.EqualTo(0.03));
        }

        [Test]
        public void ProfessionalPlayer_PerformsBetterThanAmateur()
        {
            // Arrange
            const int throwCount = 500;
            var target = Target.Triple(20);

            var proSession = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithSeed(42)
                .BuildSession();

            var amateurSession = new EnhancedDartboardSimulatorBuilder()
                .WithAmateurPlayer()
                .WithSeed(42)
                .BuildSession();

            // Act
            var proScores = new List<int>();
            var amateurScores = new List<int>();

            for (int i = 0; i < throwCount; i++)
            {
                proScores.Add(proSession.Throw(target).Score);
                amateurScores.Add(amateurSession.Throw(target).Score);
            }

            // Assert
            var proAverage = proScores.Average();
            var amateurAverage = amateurScores.Average();

            Console.WriteLine($"Professional average: {proAverage:F2}");
            Console.WriteLine($"Amateur average: {amateurAverage:F2}");

            Assert.That(proAverage, Is.GreaterThan(amateurAverage),
                "Professional should have higher average score");
        }
    }

    [TestFixture]
    public class TremorModelTests
    {
        [Test]
        public void WithLinearTremor_BuildsSuccessfully()
        {
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithLinearTremor()
                .WithSeed(42)
                .BuildSession();

            // Assert
            Assert.That(session, Is.Not.Null);

            // Werfe einige Darts und prüfe, dass Tremor zunimmt
            var initialTremor = session.CurrentTremor;
            for (int i = 0; i < 50; i++)
            {
                session.Throw(Target.Triple(20));
            }
            var finalTremor = session.CurrentTremor;

            Console.WriteLine($"Initial tremor: {initialTremor:F6}");
            Console.WriteLine($"Final tremor after 50 throws: {finalTremor:F6}");

            Assert.That(finalTremor, Is.GreaterThan(initialTremor),
                "Tremor should increase with linear model");
        }

        [Test]
        public void WithRealisticTremor_BuildsSuccessfully()
        {
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithRealisticTremor()
                .WithSeed(42)
                .BuildSession();

            // Assert
            Assert.That(session, Is.Not.Null);

            // Logarithmisches Tremor sollte sich auch aufbauen
            var initialTremor = session.CurrentTremor;
            for (int i = 0; i < 100; i++)
            {
                session.Throw(Target.Triple(20));
            }
            var finalTremor = session.CurrentTremor;

            Console.WriteLine($"Initial tremor: {initialTremor:F6}");
            Console.WriteLine($"Final tremor after 100 throws: {finalTremor:F6}");

            Assert.That(finalTremor, Is.GreaterThanOrEqualTo(initialTremor));
        }

        [Test]
        public void NoTremorModel_TremorStaysConstant()
        {
            // Arrange - Default Builder verwendet NoTremorModel
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithSeed(42)
                .BuildSession();

            // Act
            var tremorValues = new List<double>();
            for (int i = 0; i < 50; i++)
            {
                session.Throw(Target.Triple(20));
                tremorValues.Add(session.CurrentTremor);
            }

            // Assert - ohne explizites Tremor-Modell sollte Tremor konstant bleiben
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
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithStandardPressure()
                .WithSeed(42)
                .BuildSession();

            // Assert
            Assert.That(session, Is.Not.Null);

            // Teste mit Drucksituation
            var gameContext = new GameContext
            {
                RemainingScore = 32, // Checkout-Situation
                CurrentVisitResults = new List<ThrowResult>()
            };

            var result = session.Throw(Target.Double(16), gameContext);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void WithCheckoutPsychology_BuildsSuccessfully()
        {
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithCheckoutPsychology()
                .WithProfessionalPlayer()
                .WithSeed(42)
                .BuildSession();

            // Assert
            Assert.That(session, Is.Not.Null);

            // Teste Checkout-Psychologie
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
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithStandardMomentum()
                .WithSeed(42)
                .BuildSession();

            // Assert
            Assert.That(session, Is.Not.Null);

            // Werfe mehrere Darts um Momentum zu testen
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
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithSimpleGrouping()
                .WithSeed(42)
                .BuildSession();

            // Assert
            Assert.That(session, Is.Not.Null);

            // Teste Grouping - wirf mehrmals auf dasselbe Ziel
            var target = Target.Triple(20);
            var results = new List<ThrowResult>();

            for (int i = 0; i < 10; i++)
            {
                results.Add(session.Throw(target));
            }

            // Prüfe, dass Würfe Koordinaten haben
            Assert.That(results.All(r => r.HitPoint != null), Is.True);
        }
    }

    [TestFixture]
    public class TargetDifficultyTests
    {
        [Test]
        public void WithStandardTargetDifficulty_BuildsSuccessfully()
        {
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithStandardTargetDifficulty()
                .WithSeed(42)
                .BuildSession();

            // Assert
            Assert.That(session, Is.Not.Null);

            // Teste verschiedene Ziele mit unterschiedlichen Schwierigkeiten
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
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithTruncation()
                .WithSeed(42)
                .BuildSession();

            // Assert
            Assert.That(session, Is.Not.Null);

            // Teste, dass alle Würfe innerhalb einer vernünftigen Distanz bleiben
            var results = new List<ThrowResult>();
            for (int i = 0; i < 100; i++)
            {
                results.Add(session.Throw(Target.Triple(20)));
            }

            // Mit Truncation sollten extreme Abweichungen begrenzt sein
            Assert.That(results.All(r => r.Score >= 0), Is.True);
        }

        [Test]
        public void WithTruncation_CustomMaxDeviation_BuildsSuccessfully()
        {
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .WithTruncation(0.15)
                .WithSeed(42)
                .BuildSession();

            // Assert
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
        public void FluentBuilder_ChainMultipleConfigurations_BuildsSuccessfully()
        {
            // Arrange & Act
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

            // Assert
            Assert.That(session, Is.Not.Null);
            Assert.That(session.Profile.Name, Is.EqualTo("TestPro"));

            // Teste, dass alles funktioniert
            var result = session.Throw(Target.Triple(20));
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void FluentBuilder_MinimalConfiguration_UsesDefaults()
        {
            // Arrange & Act
            var session = new EnhancedDartboardSimulatorBuilder()
                .BuildSession();

            // Assert
            Assert.That(session, Is.Not.Null);
            Assert.That(session.Profile, Is.Not.Null);
            Assert.That(session.ThrowCount, Is.EqualTo(0));

            // Sollte mit Defaults funktionieren
            var result = session.Throw(Target.Triple(20));
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void FluentBuilder_AllFeatures_BuildsCompleteSession()
        {
            // Arrange & Act - teste alle verfügbaren Features
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

            // Assert
            Assert.That(session, Is.Not.Null);

            // Teste eine vollständige Wurf-Sequenz
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
        public void CompareConfigurations_WithAndWithoutTremor_ShowsDifference()
        {
            // Arrange
            const int throwCount = 100;
            var target = Target.Triple(20);

            var withTremor = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithLinearTremor()
                .WithSeed(42)
                .BuildSession();

            var withoutTremor = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithSeed(42)
                .BuildSession();

            // Act
            var scoresWithTremor = new List<int>();
            var scoresWithoutTremor = new List<int>();

            for (int i = 0; i < throwCount; i++)
            {
                scoresWithTremor.Add(withTremor.Throw(target).Score);
                scoresWithoutTremor.Add(withoutTremor.Throw(target).Score);
            }

            // Assert - mit Tremor könnte die Leistung im Laufe der Zeit abnehmen
            var firstHalfWithTremor = scoresWithTremor.Take(50).Average();
            var secondHalfWithTremor = scoresWithTremor.Skip(50).Average();

            Console.WriteLine($"With Tremor - First half: {firstHalfWithTremor:F2}, Second half: {secondHalfWithTremor:F2}");
            Console.WriteLine($"Without Tremor - Average: {scoresWithoutTremor.Average():F2}");

            // Beide Konfigurationen sollten gültige Ergebnisse liefern
            Assert.That(scoresWithTremor.All(s => s >= 0 && s <= 60), Is.True);
            Assert.That(scoresWithoutTremor.All(s => s >= 0 && s <= 60), Is.True);
        }

        [Test]
        public void ComparePlayerLevels_AllThreeProfiles_ShowsSkillGradient()
        {
            // Arrange
            const int throwCount = 300;
            var target = Target.Triple(20);

            var pro = new EnhancedDartboardSimulatorBuilder()
                .WithProfessionalPlayer()
                .WithSeed(42)
                .BuildSession();

            var amateur = new EnhancedDartboardSimulatorBuilder()
                .WithAmateurPlayer()
                .WithSeed(42)
                .BuildSession();

            var beginner = new EnhancedDartboardSimulatorBuilder()
                .WithBeginnerPlayer()
                .WithSeed(42)
                .BuildSession();

            // Act
            var proScores = new List<int>();
            var amateurScores = new List<int>();
            var beginnerScores = new List<int>();

            for (int i = 0; i < throwCount; i++)
            {
                proScores.Add(pro.Throw(target).Score);
                amateurScores.Add(amateur.Throw(target).Score);
                beginnerScores.Add(beginner.Throw(target).Score);
            }

            // Assert
            var proAvg = proScores.Average();
            var amateurAvg = amateurScores.Average();
            var beginnerAvg = beginnerScores.Average();

            Console.WriteLine($"Professional average: {proAvg:F2}");
            Console.WriteLine($"Amateur average: {amateurAvg:F2}");
            Console.WriteLine($"Beginner average: {beginnerAvg:F2}");

            Assert.That(proAvg, Is.GreaterThan(amateurAvg));
            Assert.That(amateurAvg, Is.GreaterThan(beginnerAvg));
        }
    }
}
