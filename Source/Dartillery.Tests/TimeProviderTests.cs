using Dartillery.Core.Models;
using Dartillery.Session;
using Microsoft.Extensions.Time.Testing;

namespace Dartillery.Tests;

[TestFixture]
public class TimeProviderTests
{
    [Test]
    public void SessionStateManager_WithFakeTimeProvider_UsesProvidedTime()
    {
        var startTime = new DateTimeOffset(2024, 6, 15, 8, 0, 0, TimeSpan.Zero);
        var fakeTime = new FakeTimeProvider(startTime);

        var manager = new SessionStateManager(PlayerProfile.Amateur(), fakeTime);

        Assert.That(manager.SessionStart, Is.EqualTo(startTime.UtcDateTime));
    }

    [Test]
    public void TimeSinceLastThrow_WithFakeTimeProvider_ReflectsAdvancedTime()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var manager = new SessionStateManager(PlayerProfile.Amateur(), fakeTime);

        var result = new ThrowResult(score: 20, Core.Enums.SegmentType.Single, sectorNumber: 20,
            hitPoint: new Point2D(0, 0), aimedPoint: new Point2D(0, 0));
        manager.RecordThrow(result);

        fakeTime.Advance(TimeSpan.FromSeconds(30));

        Assert.That(manager.TimeSinceLastThrow.TotalSeconds, Is.EqualTo(30).Within(0.001));
    }

    [Test]
    public void GetCurrentState_WithFakeTimeProvider_ReportsCorrectSessionDuration()
    {
        var fakeTime = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var manager = new SessionStateManager(PlayerProfile.Amateur(), fakeTime);

        fakeTime.Advance(TimeSpan.FromMinutes(5));
        var state = manager.GetCurrentState();

        Assert.That(state.SessionDuration.TotalMinutes, Is.EqualTo(5).Within(0.001));
    }

    [Test]
    public void PlayerSession_DefaultTimeProvider_WorksWithoutExplicitProvider()
    {
        var session = new EnhancedDartboardSimulatorBuilder().WithSeed(42).BuildSession();

        Assert.Multiple(() =>
        {
            Assert.That(session.SessionId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(session.Throw(Target.Triple(20)), Is.Not.Null);
        });
    }
}
