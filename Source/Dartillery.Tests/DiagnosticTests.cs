using Dartillery;
using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;
using Dartillery.Simulation.Geometry;

namespace Dartillery.Tests;

[TestFixture]
public class DiagnosticTests
{
    [Test]
    public void DiagnoseT20_ShowsCoordinates()
    {
        // Arrange
        var target = Target.Triple(20);
        var simulator = new DartboardSimulatorBuilder()
            .WithPrecision(SimulatorPrecision.Professional)
            .WithSeed(42)
            .Build();

        Console.WriteLine("=== T20 Coordinate Diagnostics ===\n");

        // Get aim point for T20
        var aimCalc = new AimPointCalculator();
        var aimPoint = aimCalc.CalculateAimPoint(target);

        Console.WriteLine($"Aim Point for T20: ({aimPoint.X:F6}, {aimPoint.Y:F6})");
        Console.WriteLine($"Aim Point Polar: radius={Math.Sqrt(aimPoint.X * aimPoint.X + aimPoint.Y * aimPoint.Y):F6}, angle={Math.Atan2(aimPoint.Y, aimPoint.X) * 180 / Math.PI:F2}°\n");

        // Simulate 20 throws and show coordinates
        Console.WriteLine("First 20 Throws:");
        Console.WriteLine($"{"#",-4} {"dx",-10} {"dy",-10} {"hitX",-10} {"hitY",-10} {"Result",-15}");
        Console.WriteLine(new string('-', 70));

        for (int i = 0; i < 20; i++)
        {
            var result = simulator.Throw(target);
            double hitX = result.HitPoint.X;
            double hitY = result.HitPoint.Y;
            double dx = hitX - aimPoint.X;
            double dy = hitY - aimPoint.Y;

            string segmentDesc = result.SegmentType == Core.Enums.SegmentType.Miss
                ? "Miss"
                : $"{result.SegmentType} {result.SectorNumber}";

            Console.WriteLine($"{i+1,-4} {dx,-10:F6} {dy,-10:F6} {hitX,-10:F6} {hitY,-10:F6} {segmentDesc,-15}");
        }

        // Analyze deviation distribution
        Console.WriteLine("\n=== Analyzing 1000 throws for deviation bias ===\n");

        var simulator2 = new DartboardSimulatorBuilder()
            .WithPrecision(SimulatorPrecision.Professional)
            .WithSeed(999)
            .Build();

        double sumDx = 0;
        double sumDy = 0;
        double sumAbsDx = 0;
        double sumAbsDy = 0;
        int positiveX = 0;
        int negativeX = 0;
        int positiveY = 0;
        int negativeY = 0;

        for (int i = 0; i < 1000; i++)
        {
            var result = simulator2.Throw(target);
            double dx = result.HitPoint.X - aimPoint.X;
            double dy = result.HitPoint.Y - aimPoint.Y;

            sumDx += dx;
            sumDy += dy;
            sumAbsDx += Math.Abs(dx);
            sumAbsDy += Math.Abs(dy);

            if (dx > 0) positiveX++;
            else if (dx < 0) negativeX++;

            if (dy > 0) positiveY++;
            else if (dy < 0) negativeY++;
        }

        Console.WriteLine($"Mean dx: {sumDx / 1000:F6} (should be ~0)");
        Console.WriteLine($"Mean dy: {sumDy / 1000:F6} (should be ~0)");
        Console.WriteLine($"Mean |dx|: {sumAbsDx / 1000:F6}");
        Console.WriteLine($"Mean |dy|: {sumAbsDy / 1000:F6}");
        Console.WriteLine($"\nX direction: {positiveX} positive, {negativeX} negative (should be ~50/50)");
        Console.WriteLine($"Y direction: {positiveY} positive, {negativeY} negative (should be ~50/50)");
    }

    [Test]
    public void TestSectorAngles_ShowsAllSectors()
    {
        Console.WriteLine("=== Dartboard Sector Angles ===\n");
        Console.WriteLine($"{"Sector",-8} {"Index",-8} {"Angle (rad)",-15} {"Angle (deg)",-12} {"Left Neighbor",-15} {"Right Neighbor",-15}");
        Console.WriteLine(new string('-', 85));

        var sectors = new[] { 20, 1, 18, 4, 13, 6, 10, 15, 2, 17, 3, 19, 7, 16, 8, 11, 14, 9, 12, 5 };

        for (int i = 0; i < sectors.Length; i++)
        {
            int sector = sectors[i];
            double angleRad = i * (2 * Math.PI / 20);
            double angleDeg = angleRad * 180 / Math.PI;

            int leftNeighborIdx = (i - 1 + 20) % 20;
            int rightNeighborIdx = (i + 1) % 20;

            Console.WriteLine($"{sector,-8} {i,-8} {angleRad,-15:F4} {angleDeg,-12:F2} {sectors[leftNeighborIdx],-15} {sectors[rightNeighborIdx],-15}");
        }
    }
}
