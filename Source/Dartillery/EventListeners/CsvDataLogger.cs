using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.EventListeners;

/// <summary>
/// Logs throw data to CSV for bot training / analysis.
/// Creates a CSV file with detailed throw information for machine learning.
/// </summary>
public sealed class CsvDataLogger : IThrowEventListener, IDisposable
{
    private readonly StreamWriter _writer;

    /// <summary>
    /// Creates a CSV data logger.
    /// </summary>
    /// <param name="filePath">Path to CSV file (will be created/appended).</param>
    public CsvDataLogger(string filePath)
    {
        _writer = new StreamWriter(filePath, append: true);

        // Write header if new file
        if (new FileInfo(filePath).Length == 0)
        {
            _writer.WriteLine("Timestamp,SessionId,ThrowIndex,PlayerName,BaseSkill," +
                "Tremor,Pressure,Momentum,AimX,AimY,HitX,HitY," +
                "Score,SegmentType,SectorNumber");
        }
    }

    /// <inheritdoc />
    public void OnThrowCompleted(ThrowEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);
        _writer.WriteLine($"{evt.Timestamp:O}," +
            $"{evt.SessionId}," +
            $"{evt.Context.ThrowIndexInSession}," +
            $"{evt.Profile.Name}," +
            $"{evt.Profile.BaseSkill}," +
            $"{evt.Context.SessionTremor}," +
            $"{evt.Context.PressureModifier}," +
            $"{evt.Context.MomentumModifier}," +
            $"{evt.Result.AimedPoint.X}," +
            $"{evt.Result.AimedPoint.Y}," +
            $"{evt.Result.HitPoint.X}," +
            $"{evt.Result.HitPoint.Y}," +
            $"{evt.Result.Score}," +
            $"{evt.Result.SegmentType}," +
            $"{evt.Result.SectorNumber}");
        _writer.Flush();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _writer?.Dispose();
    }
}
