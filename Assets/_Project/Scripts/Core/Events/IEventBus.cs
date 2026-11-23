using System;

namespace WheelGame.Core.Events
{
    public interface IEventBus
    {
        void Subscribe<TEvent>(Action<TEvent> handler);

        void Unsubscribe<TEvent>(Action<TEvent> handler);

        void Publish<TEvent>(TEvent message);
    }
}
