using System;
using NUnit.Framework;
using WheelGame.Core.Events;

namespace WheelGame.Tests.EditMode
{
    public sealed class EventBusTests
    {
        private readonly struct PingEvent
        {
            public PingEvent(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        private readonly struct OtherEvent
        {
        }

        [Test]
        public void Publish_DeliversToSubscribersOfThatTypeOnly()
        {
            var bus = new EventBus();
            var received = 0;
            var otherReceived = 0;
            bus.Subscribe<PingEvent>(message => received = message.Value);
            bus.Subscribe<OtherEvent>(message => otherReceived++);

            bus.Publish(new PingEvent(7));

            Assert.AreEqual(7, received);
            Assert.AreEqual(0, otherReceived);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            var bus = new EventBus();
            var count = 0;
            Action<PingEvent> handler = message => count++;
            bus.Subscribe(handler);

            bus.Unsubscribe(handler);
            bus.Publish(new PingEvent(1));

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Subscribe_SameHandlerTwice_DeliversOnce()
        {
            var bus = new EventBus();
            var count = 0;
            Action<PingEvent> handler = message => count++;
            bus.Subscribe(handler);
            bus.Subscribe(handler);

            bus.Publish(new PingEvent(1));

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Unsubscribe_DuringPublish_DoesNotBreakTheLoop()
        {
            var bus = new EventBus();
            var secondCount = 0;
            Action<PingEvent> first = null;
            first = message => bus.Unsubscribe(first);
            bus.Subscribe(first);
            bus.Subscribe<PingEvent>(message => secondCount++);

            bus.Publish(new PingEvent(1));
            bus.Publish(new PingEvent(2));

            Assert.AreEqual(2, secondCount);
        }

        [Test]
        public void Publish_WithoutSubscribers_DoesNothing()
        {
            Assert.DoesNotThrow(() => new EventBus().Publish(new PingEvent(1)));
        }
    }
}
