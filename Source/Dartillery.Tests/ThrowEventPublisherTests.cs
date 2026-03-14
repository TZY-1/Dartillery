using Dartillery.Core.Abstractions;
using Dartillery.Core.Enums;
using Dartillery.Core.Models;
using Dartillery.Session;

namespace Dartillery.Tests;

[TestFixture]
public class ThrowEventPublisherTests
{
    private sealed class LambdaThrowEventListener : IThrowEventListener
    {
        private readonly Action<ThrowEvent> _action;

        public LambdaThrowEventListener(Action<ThrowEvent> action)
        {
            _action = action;
        }

        public void OnThrowCompleted(ThrowEvent throwEvent) => _action(throwEvent);
    }

    private static ThrowResult SomeResult() =>
        new(score: 20, SegmentType.Single, sectorNumber: 20,
            hitPoint: new Point2D(0, 0), aimedPoint: new Point2D(0, 0));

    private static ThrowContext SomeContext() => ThrowContext.Neutral;

    private static PlayerProfile SomeProfile() => new();

    [Test]
    public void Publish_ThrowingListener_DoesNotPreventSubsequentListenersFromExecuting()
    {
        // Arrange
        bool secondListenerCalled = false;

        var firstListener = new LambdaThrowEventListener(_ => throw new InvalidOperationException("First listener exploded"));
        var secondListener = new LambdaThrowEventListener(_ => secondListenerCalled = true);

        var publisher = new ThrowEventPublisher(new[] { firstListener, secondListener });

        // Act
        Assert.Throws<AggregateException>(() =>
            publisher.Publish(SomeResult(), SomeContext(), SomeProfile(), sessionId: 1L, DateTime.UtcNow));

        // Assert
        Assert.That(secondListenerCalled, Is.True, "Second listener should have been called even though first listener threw");
    }

    [Test]
    public void Publish_TwoThrowingListeners_AggregateExceptionContainsBothErrors()
    {
        // Arrange
        var error1 = new InvalidOperationException("Error from listener 1");
        var error2 = new ArgumentException("Error from listener 2");

        var firstListener = new LambdaThrowEventListener(_ => throw error1);
        var secondListener = new LambdaThrowEventListener(_ => throw error2);

        var publisher = new ThrowEventPublisher(new[] { firstListener, secondListener });

        // Act
        var aggregate = Assert.Throws<AggregateException>(() =>
            publisher.Publish(SomeResult(), SomeContext(), SomeProfile(), sessionId: 1L, DateTime.UtcNow));

        // Assert
        Assert.That(aggregate, Is.Not.Null);
        Assert.That(aggregate!.InnerExceptions, Has.Count.EqualTo(2));
        Assert.That(aggregate.InnerExceptions, Does.Contain(error1));
        Assert.That(aggregate.InnerExceptions, Does.Contain(error2));
    }

    [Test]
    public void Publish_NoThrowingListeners_NoExceptionThrown()
    {
        // Arrange
        int callCount = 0;
        var listener1 = new LambdaThrowEventListener(_ => callCount++);
        var listener2 = new LambdaThrowEventListener(_ => callCount++);

        var publisher = new ThrowEventPublisher(new[] { listener1, listener2 });

        // Act & Assert
        Assert.DoesNotThrow(() =>
            publisher.Publish(SomeResult(), SomeContext(), SomeProfile(), sessionId: 1L, DateTime.UtcNow));

        Assert.That(callCount, Is.EqualTo(2), "Both listeners should have been called");
    }
}
