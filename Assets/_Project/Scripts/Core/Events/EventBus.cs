using System;
using System.Collections.Generic;

namespace WheelGame.Core.Events
{
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
            {
                handlers = new List<Delegate>();
                _handlers.Add(typeof(TEvent), handlers);
            }

            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                return;
            }

            if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        public void Publish<TEvent>(TEvent message)
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
            {
                return;
            }

            if (handlers.Count == 0)
            {
                return;
            }

            var snapshot = handlers.ToArray();

            foreach (var handler in snapshot)
            {
                ((Action<TEvent>)handler).Invoke(message);
            }
        }
    }
}
