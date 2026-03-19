using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;
using Dartillery.Session;

namespace Dartillery.Tests;

[TestFixture]
public class ThrowEventPublisherTests
{
    [Test]
    public void Publish_ThrowingListener_DoesNotPreventSubsequentListenersFromExecuting()
    {
        bool secondListenerCalled = false;

        var firstListener = new LambdaThrowEventListener(_ => throw new InvalidOperationException("First listener exploded"));
        var secondListener = new LambdaThrowEventListener(_ => secondListenerCalled = true);

        var publisher = new ThrowEventPublisher(new[] { firstListener, secondListener });

        Assert.Throws<AggregateException>(() =>
            publisher.Publish(SomeResult(), SomeContext(), SomeProfile(), sessionId: Guid.NewGuid(), DateTime.UtcNow));

        Assert.That(secondListenerCalled, Is.True, "Second listener should have been called even though first listener threw");
    }

    [Test]
    public void Publish_TwoThrowingListeners_AggregateExceptionContainsBothErrors()
    {
        var error1 = new InvalidOperationException("Error from listener 1");
        var error2 = new ArgumentException("Error from listener 2");

        var firstListener = new LambdaThrowEventListener(_ => throw error1);
        var secondListener = new LambdaThrowEventListener(_ => throw error2);

        var publisher = new ThrowEventPublisher(new[] { firstListener, secondListener });

        var aggregate = Assert.Throws<AggregateException>(() =>
            publisher.Publish(SomeResult(), SomeContext(), SomeProfile(), sessionId: Guid.NewGuid(), DateTime.UtcNow));

        Assert.That(aggregate, Is.Not.Null);
        Assert.That(aggregate!.InnerExceptions, Has.Count.EqualTo(2));
        Assert.That(aggregate.InnerExceptions, Does.Contain(error1));
        Assert.That(aggregate.InnerExceptions, Does.Contain(error2));
    }

    [Test]
    public void Publish_NoThrowingListeners_NoExceptionThrown()
    {
        int callCount = 0;
        var listener1 = new LambdaThrowEventListener(_ => callCount++);
        var listener2 = new LambdaThrowEventListener(_ => callCount++);

        var publisher = new ThrowEventPublisher(new[] { listener1, listener2 });

        Assert.DoesNotThrow(() =>
            publisher.Publish(SomeResult(), SomeContext(), SomeProfile(), sessionId: Guid.NewGuid(), DateTime.UtcNow));

        Assert.That(callCount, Is.EqualTo(2), "Both listeners should have been called");
    }

    private static ThrowResult SomeResult() =>
        new(score: 20, SegmentType.Single, sectorNumber: 20,
            hitPoint: new Point2D(0, 0), aimedPoint: new Point2D(0, 0));

    private static ThrowContext SomeContext() => ThrowContext.Neutral;

    private static PlayerProfile SomeProfile() => new();

    private sealed class LambdaThrowEventListener : IThrowEventListener
    {
        private readonly Action<ThrowEvent> _action;

        public LambdaThrowEventListener(Action<ThrowEvent> action)
        {
            _action = action;
        }

        public void OnThrowCompleted(ThrowEvent evt) => _action(evt);
    }
}
