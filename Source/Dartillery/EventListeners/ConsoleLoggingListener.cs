using Dartillery.Core.Abstractions;
using Dartillery.Core.Models;

namespace Dartillery.EventListeners;

/// <summary>
/// Simple console logging for throw events.
/// Useful for debugging and real-time monitoring.
/// </summary>
public sealed class ConsoleLoggingListener : IThrowEventListener
{
    /// <inheritdoc />
    public void OnThrowCompleted(ThrowEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);
        Console.WriteLine($"[{evt.Timestamp:HH:mm:ss}] {evt.Profile.Name}: " +
            $"{evt.Result.SegmentType} {evt.Result.SectorNumber} " +
            $"(Score: {evt.Result.Score}, Fatigue: {evt.Context.SessionFatigue:F4})");
    }
}
