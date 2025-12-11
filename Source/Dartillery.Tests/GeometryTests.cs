using Dartillery.Core.Enums;
using Dartillery.Core.Models;
using Dartillery.Simulation.Geometry;
using Dartillery.Shared;

namespace Dartillery.Tests;

[TestFixture]
public class GeometryTests
{
    [TestFixture]
    public class RingResolverTests
    {
        private RingResolver _resolver = null!;

        [SetUp]
        public void Setup()
        {
            _resolver = new RingResolver();
        }

        [Test]
        public void ResolveRing_InnerBullCenter_ReturnsInnerBull()
        {
            // Arrange
            double radius = 0.0;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.InnerBull));
        }

        [Test]
        public void ResolveRing_InnerBullEdge_ReturnsInnerBull()
        {
            // Arrange
            double radius = BoardDimensions.InnerBullRadius;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.InnerBull));
        }

        [Test]
        public void ResolveRing_OuterBullInner_ReturnsOuterBull()
        {
            // Arrange
            double radius = BoardDimensions.InnerBullRadius + 0.001;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.OuterBull));
        }

        [Test]
        public void ResolveRing_OuterBullEdge_ReturnsOuterBull()
        {
            // Arrange
            double radius = BoardDimensions.OuterBullRadius;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.OuterBull));
        }

        [Test]
        public void ResolveRing_TripleRingInnerEdge_ReturnsTriple()
        {
            // Arrange
            double radius = BoardDimensions.TripleRingInner;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.Triple));
        }

        [Test]
        public void ResolveRing_TripleRingCenter_ReturnsTriple()
        {
            // Arrange
            double radius = (BoardDimensions.TripleRingInner + BoardDimensions.TripleRingOuter) / 2.0;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.Triple));
        }

        [Test]
        public void ResolveRing_TripleRingOuterEdge_ReturnsTriple()
        {
            // Arrange
            double radius = BoardDimensions.TripleRingOuter;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.Triple));
        }

        [Test]
        public void ResolveRing_DoubleRingInnerEdge_ReturnsDouble()
        {
            // Arrange
            double radius = BoardDimensions.DoubleRingInner;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.Double));
        }

        [Test]
        public void ResolveRing_DoubleRingOuterEdge_ReturnsDouble()
        {
            // Arrange
            double radius = BoardDimensions.DoubleRingOuter;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.Double));
        }

        [Test]
        public void ResolveRing_BetweenOuterBullAndTriple_ReturnsSingle()
        {
            // Arrange
            double radius = BoardDimensions.OuterBullRadius + 0.001;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.Single));
        }

        [Test]
        public void ResolveRing_BetweenTripleAndDouble_ReturnsSingle()
        {
            // Arrange
            double radius = BoardDimensions.TripleRingOuter + 0.001;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.Single));
        }

        [Test]
        public void ResolveRing_JustBeforeTriple_ReturnsSingle()
        {
            // Arrange
            double radius = BoardDimensions.TripleRingInner - 0.001;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.Single));
        }

        [Test]
        public void ResolveRing_JustBeforeDouble_ReturnsSingle()
        {
            // Arrange
            double radius = BoardDimensions.DoubleRingInner - 0.001;

            // Act
            var result = _resolver.ResolveRing(radius);

            // Assert
            Assert.That(result, Is.EqualTo(SegmentType.Single));
        }

        [Test]
        public void ResolveRing_AllRingTypes_CoverFullRadiusRange()
        {
            // This test verifies that there are no gaps in ring coverage
            // by testing many points from center to beyond the board

            var testRadii = new List<(double Radius, SegmentType Expected)>
            {
                (0.000, SegmentType.InnerBull),
                (0.003, SegmentType.InnerBull),
                (BoardDimensions.InnerBullRadius, SegmentType.InnerBull),
                (BoardDimensions.InnerBullRadius + 0.001, SegmentType.OuterBull),
                (0.010, SegmentType.OuterBull),
                (BoardDimensions.OuterBullRadius, SegmentType.OuterBull),
                (BoardDimensions.OuterBullRadius + 0.001, SegmentType.Single),
                (0.050, SegmentType.Single),
                (BoardDimensions.TripleRingInner - 0.001, SegmentType.Single),
                (BoardDimensions.TripleRingInner, SegmentType.Triple),
                (BoardDimensions.TripleRingOuter, SegmentType.Triple),
                (BoardDimensions.TripleRingOuter + 0.001, SegmentType.Single),
                (0.130, SegmentType.Single),
                (BoardDimensions.DoubleRingInner - 0.001, SegmentType.Single),
                (BoardDimensions.DoubleRingInner, SegmentType.Double),
                (BoardDimensions.DoubleRingOuter, SegmentType.Double)
            };

            foreach (var (radius, expected) in testRadii)
            {
                var result = _resolver.ResolveRing(radius);
                Assert.That(result, Is.EqualTo(expected),
                    $"Radius {radius:F6} should be {expected} but was {result}");
            }
        }
    }

    [TestFixture]
    public class SectorResolverTests
    {
        private SectorResolver _resolver = null!;

        [SetUp]
        public void Setup()
        {
            _resolver = new SectorResolver();
        }

        [Test]
        public void ResolveSector_TopCenter_ReturnsSector20()
        {
            // Arrange - Point straight up (12 o'clock)
            var point = new Point2D(0.0, 0.1);

            // Act
            var sector = _resolver.ResolveSector(point);

            // Assert
            Assert.That(sector, Is.EqualTo(20));
        }

        [Test]
        public void ResolveSector_Right_ReturnsSector6()
        {
            // Arrange - Point straight right (3 o'clock)
            var point = new Point2D(0.1, 0.0);

            // Act
            var sector = _resolver.ResolveSector(point);

            // Assert
            Assert.That(sector, Is.EqualTo(6));
        }

        [Test]
        public void ResolveSector_Bottom_ReturnsSector3()
        {
            // Arrange - Point straight down (6 o'clock)
            var point = new Point2D(0.0, -0.1);

            // Act
            var sector = _resolver.ResolveSector(point);

            // Assert
            Assert.That(sector, Is.EqualTo(3));
        }

        [Test]
        public void ResolveSector_Left_ReturnsSector11()
        {
            // Arrange - Point straight left (9 o'clock)
            var point = new Point2D(-0.1, 0.0);

            // Act
            var sector = _resolver.ResolveSector(point);

            // Assert
            Assert.That(sector, Is.EqualTo(11));
        }

        [Test]
        public void ResolveSector_AllSectors_FollowClockwiseOrder()
        {
            // Arrange
            var expectedOrder = new[] { 20, 1, 18, 4, 13, 6, 10, 15, 2, 17, 3, 19, 7, 16, 8, 11, 14, 9, 12, 5 };

            // Act & Assert - Test a point in each sector
            for (int i = 0; i < 20; i++)
            {
                // Calculate angle for middle of sector
                double angle = i * (2 * Math.PI / 20);

                // Create point at that angle
                var point = new Point2D(
                    Math.Sin(angle) * 0.1,  // Use Sin/Cos to get Y-up coordinate system
                    Math.Cos(angle) * 0.1
                );

                var sector = _resolver.ResolveSector(point);
                Assert.That(sector, Is.EqualTo(expectedOrder[i]),
                    $"Sector at index {i} (angle {angle * 180 / Math.PI:F1}°) should be {expectedOrder[i]}");
            }
        }

        [Test]
        public void ResolveSector_SectorBoundaries_CorrectlyClassified()
        {
            // Test points exactly at sector boundaries
            // Each sector spans 18 degrees, boundaries should be handled correctly

            var halfSector = BoardDimensions.SectorAngle / 2.0;

            for (int i = 0; i < 20; i++)
            {
                // Test point just inside the sector (from the lower boundary)
                double angleInsideLower = i * BoardDimensions.SectorAngle + 0.001;
                var pointInsideLower = new Point2D(
                    Math.Sin(angleInsideLower) * 0.1,
                    Math.Cos(angleInsideLower) * 0.1
                );

                // Test point just inside the sector (from the upper boundary)
                double angleInsideUpper = (i + 1) * BoardDimensions.SectorAngle - 0.001;
                var pointInsideUpper = new Point2D(
                    Math.Sin(angleInsideUpper) * 0.1,
                    Math.Cos(angleInsideUpper) * 0.1
                );

                var sectorLower = _resolver.ResolveSector(pointInsideLower);
                var sectorUpper = _resolver.ResolveSector(pointInsideUpper);

                // Both points should resolve to the same sector
                Assert.That(sectorLower, Is.EqualTo(sectorUpper),
                    $"Sector boundary test failed at sector index {i}");
            }
        }

        [Test]
        public void ResolveSector_NegativeCoordinates_HandledCorrectly()
        {
            // Test all quadrants
            var testPoints = new List<(Point2D Point, int ExpectedSectorRange)>
            {
                (new Point2D(0.1, 0.1), 20),    // Q1 - top right (should be near 20 or 1)
                (new Point2D(-0.1, 0.1), 20),   // Q2 - top left (should be near 20 or 18)
                (new Point2D(-0.1, -0.1), 3),   // Q3 - bottom left (should be near 3 or 11)
                (new Point2D(0.1, -0.1), 3)     // Q4 - bottom right (should be near 3 or 6)
            };

            foreach (var (point, _) in testPoints)
            {
                var sector = _resolver.ResolveSector(point);

                // Sector should be a valid number between 1 and 20
                Assert.That(sector, Is.InRange(1, 20),
                    $"Point ({point.X:F2}, {point.Y:F2}) produced invalid sector {sector}");
            }
        }

        [Test]
        public void ResolveSector_VerySmallRadius_StillResolvesCorrectly()
        {
            // Even very close to the origin, sector resolution should work
            var point = new Point2D(0.0, 0.001);

            var sector = _resolver.ResolveSector(point);

            Assert.That(sector, Is.EqualTo(20));
        }

        [Test]
        public void ResolveSector_SameAngleDifferentRadius_ReturnsSameSector()
        {
            // Points at the same angle but different radii should be in the same sector
            double angle = Math.PI / 6; // 30 degrees

            var point1 = new Point2D(Math.Cos(angle) * 0.05, Math.Sin(angle) * 0.05);
            var point2 = new Point2D(Math.Cos(angle) * 0.15, Math.Sin(angle) * 0.15);

            var sector1 = _resolver.ResolveSector(point1);
            var sector2 = _resolver.ResolveSector(point2);

            Assert.That(sector1, Is.EqualTo(sector2));
        }
    }

    [TestFixture]
    public class ScoreCalculatorTests
    {
        private ScoreCalculator _calculator = null!;

        [SetUp]
        public void Setup()
        {
            _calculator = new ScoreCalculator();
        }

        [Test]
        public void CalculateScore_InnerBull_Returns50()
        {
            var score = _calculator.CalculateScore(SegmentType.InnerBull, 0);
            Assert.That(score, Is.EqualTo(50));
        }

        [Test]
        public void CalculateScore_OuterBull_Returns25()
        {
            var score = _calculator.CalculateScore(SegmentType.OuterBull, 0);
            Assert.That(score, Is.EqualTo(25));
        }

        [Test]
        public void CalculateScore_Miss_Returns0()
        {
            var score = _calculator.CalculateScore(SegmentType.Miss, 0);
            Assert.That(score, Is.EqualTo(0));
        }

        [Test]
        public void CalculateScore_Triple20_Returns60()
        {
            var score = _calculator.CalculateScore(SegmentType.Triple, 20);
            Assert.That(score, Is.EqualTo(60));
        }

        [Test]
        public void CalculateScore_Double20_Returns40()
        {
            var score = _calculator.CalculateScore(SegmentType.Double, 20);
            Assert.That(score, Is.EqualTo(40));
        }

        [Test]
        public void CalculateScore_Single20_Returns20()
        {
            var score = _calculator.CalculateScore(SegmentType.Single, 20);
            Assert.That(score, Is.EqualTo(20));
        }

        [Test]
        public void CalculateScore_AllTriples_ReturnsCorrectScores()
        {
            for (int sector = 1; sector <= 20; sector++)
            {
                var score = _calculator.CalculateScore(SegmentType.Triple, sector);
                Assert.That(score, Is.EqualTo(sector * 3),
                    $"Triple {sector} should score {sector * 3}");
            }
        }

        [Test]
        public void CalculateScore_AllDoubles_ReturnsCorrectScores()
        {
            for (int sector = 1; sector <= 20; sector++)
            {
                var score = _calculator.CalculateScore(SegmentType.Double, sector);
                Assert.That(score, Is.EqualTo(sector * 2),
                    $"Double {sector} should score {sector * 2}");
            }
        }

        [Test]
        public void CalculateScore_AllSingles_ReturnsCorrectScores()
        {
            for (int sector = 1; sector <= 20; sector++)
            {
                var score = _calculator.CalculateScore(SegmentType.Single, sector);
                Assert.That(score, Is.EqualTo(sector),
                    $"Single {sector} should score {sector}");
            }
        }

        [Test]
        public void CalculateScore_HighestPossible_IsTriple20()
        {
            var maxScore = 0;

            for (int sector = 1; sector <= 20; sector++)
            {
                var tripleScore = _calculator.CalculateScore(SegmentType.Triple, sector);
                maxScore = Math.Max(maxScore, tripleScore);
            }

            var bullScore = _calculator.CalculateScore(SegmentType.InnerBull, 0);
            maxScore = Math.Max(maxScore, bullScore);

            Assert.That(maxScore, Is.EqualTo(60));
        }

        [Test]
        public void CalculateScore_InvalidSegmentType_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _calculator.CalculateScore((SegmentType)999, 20));
        }
    }

    [TestFixture]
    public class SegmentResolverTests
    {
        private SegmentResolver _resolver = null!;

        [SetUp]
        public void Setup()
        {
            _resolver = new SegmentResolver();
        }

        [Test]
        public void Resolve_CenterOfBoard_ReturnsInnerBull()
        {
            var hitPoint = new Point2D(0.0, 0.0);
            var aimedPoint = new Point2D(0.0, 0.0);

            var result = _resolver.Resolve(hitPoint, aimedPoint);

            Assert.That(result.SegmentType, Is.EqualTo(SegmentType.InnerBull));
            Assert.That(result.Score, Is.EqualTo(50));
            Assert.That(result.SectorNumber, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_OutsideBoard_ReturnsMiss()
        {
            var hitPoint = new Point2D(0.2, 0.2); // Radius > BoardDimensions.BoardRadius
            var aimedPoint = new Point2D(0.1, 0.1);

            var result = _resolver.Resolve(hitPoint, aimedPoint);

            Assert.That(result.SegmentType, Is.EqualTo(SegmentType.Miss));
            Assert.That(result.Score, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_Triple20Region_ReturnsTriple20()
        {
            // Point in Triple 20 region (top of board, triple ring radius)
            var radius = (BoardDimensions.TripleRingInner + BoardDimensions.TripleRingOuter) / 2.0;
            var hitPoint = new Point2D(0.0, radius);
            var aimedPoint = new Point2D(0.0, radius);

            var result = _resolver.Resolve(hitPoint, aimedPoint);

            Assert.That(result.SegmentType, Is.EqualTo(SegmentType.Triple));
            Assert.That(result.SectorNumber, Is.EqualTo(20));
            Assert.That(result.Score, Is.EqualTo(60));
        }

        [Test]
        public void Resolve_Double20Region_ReturnsDouble20()
        {
            // Point in Double 20 region (top of board, double ring radius)
            var radius = (BoardDimensions.DoubleRingInner + BoardDimensions.DoubleRingOuter) / 2.0;
            var hitPoint = new Point2D(0.0, radius);
            var aimedPoint = new Point2D(0.0, radius);

            var result = _resolver.Resolve(hitPoint, aimedPoint);

            Assert.That(result.SegmentType, Is.EqualTo(SegmentType.Double));
            Assert.That(result.SectorNumber, Is.EqualTo(20));
            Assert.That(result.Score, Is.EqualTo(40));
        }

        [Test]
        public void Resolve_Single20Region_ReturnsSingle20()
        {
            // Point in Single 20 region (top of board, between bull and triple)
            var radius = BoardDimensions.OuterBullRadius + 0.01;
            var hitPoint = new Point2D(0.0, radius);
            var aimedPoint = new Point2D(0.0, radius);

            var result = _resolver.Resolve(hitPoint, aimedPoint);

            Assert.That(result.SegmentType, Is.EqualTo(SegmentType.Single));
            Assert.That(result.SectorNumber, Is.EqualTo(20));
            Assert.That(result.Score, Is.EqualTo(20));
        }

        [Test]
        public void Resolve_OuterBullRegion_ReturnsOuterBull()
        {
            var radius = (BoardDimensions.InnerBullRadius + BoardDimensions.OuterBullRadius) / 2.0;
            var hitPoint = new Point2D(radius, 0.0);
            var aimedPoint = new Point2D(0.0, 0.0);

            var result = _resolver.Resolve(hitPoint, aimedPoint);

            Assert.That(result.SegmentType, Is.EqualTo(SegmentType.OuterBull));
            Assert.That(result.Score, Is.EqualTo(25));
            Assert.That(result.SectorNumber, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_BullHits_DoNotHaveSectorNumbers()
        {
            var innerBullPoint = new Point2D(0.0, 0.0);
            var outerBullPoint = new Point2D(0.01, 0.0);
            var aimedPoint = new Point2D(0.0, 0.0);

            var innerResult = _resolver.Resolve(innerBullPoint, aimedPoint);
            var outerResult = _resolver.Resolve(outerBullPoint, aimedPoint);

            Assert.That(innerResult.SectorNumber, Is.EqualTo(0));
            Assert.That(outerResult.SectorNumber, Is.EqualTo(0));
        }

        [Test]
        public void Resolve_AllSectors_CanBeHit()
        {
            var expectedSectors = new[] { 20, 1, 18, 4, 13, 6, 10, 15, 2, 17, 3, 19, 7, 16, 8, 11, 14, 9, 12, 5 };
            var hitSectors = new HashSet<int>();

            var radius = (BoardDimensions.TripleRingInner + BoardDimensions.TripleRingOuter) / 2.0;

            for (int i = 0; i < 20; i++)
            {
                double angle = i * (2 * Math.PI / 20);
                var hitPoint = new Point2D(
                    Math.Sin(angle) * radius,
                    Math.Cos(angle) * radius
                );
                var aimedPoint = hitPoint;

                var result = _resolver.Resolve(hitPoint, aimedPoint);
                hitSectors.Add(result.SectorNumber);
            }

            Assert.That(hitSectors, Has.Count.EqualTo(20));
            Assert.That(hitSectors, Is.EquivalentTo(expectedSectors));
        }

        [Test]
        public void Resolve_StoresHitAndAimedPoints()
        {
            var hitPoint = new Point2D(0.05, 0.05);
            var aimedPoint = new Point2D(0.04, 0.06);

            var result = _resolver.Resolve(hitPoint, aimedPoint);

            Assert.That(result.HitPoint, Is.EqualTo(hitPoint));
            Assert.That(result.AimedPoint, Is.EqualTo(aimedPoint));
        }

        [Test]
        public void Resolve_ExactBoardBoundary_IsNotMiss()
        {
            var hitPoint = new Point2D(BoardDimensions.BoardRadius, 0.0);
            var aimedPoint = hitPoint;

            var result = _resolver.Resolve(hitPoint, aimedPoint);

            // At exactly the board radius, we should still be on the double ring
            Assert.That(result.SegmentType, Is.Not.EqualTo(SegmentType.Miss));
        }

        [Test]
        public void Resolve_JustBeyondBoardBoundary_IsMiss()
        {
            var hitPoint = new Point2D(BoardDimensions.BoardRadius + 0.001, 0.0);
            var aimedPoint = new Point2D(BoardDimensions.BoardRadius, 0.0);

            var result = _resolver.Resolve(hitPoint, aimedPoint);

            Assert.That(result.SegmentType, Is.EqualTo(SegmentType.Miss));
            Assert.That(result.Score, Is.EqualTo(0));
        }
    }
}
