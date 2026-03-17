using System;
using System.Collections.Generic;

namespace _KIT.Event
{
    /// <summary>
    /// Dùng an toàn cho UI
    /// </summary>
    internal sealed class EventBus : IDisposable
    {
        private static EventBus instance;

        public static EventBus Instance
        {
            get
            {
                if (instance == null) instance = new EventBus();
                return instance;
            }
        }

        // Mỗi event type giữ đúng Action<T>
        private readonly Dictionary<Type, Delegate> listeners = new Dictionary<Type, Delegate>(32);

        // --------------------
        // PUBLISH
        // --------------------
        public void Publish<T>(T evt) where T : struct, IEvent
        {
            if (listeners.TryGetValue(typeof(T), out var del))
            {
                ((Action<T>)del)?.Invoke(evt);
            }
        }

        // --------------------
        // SUBSCRIBE
        // --------------------
        public void Subscribe<T>(Action<T> callback) where T : struct, IEvent
        {
            if (callback == null) return;

            var type = typeof(T);

            if (listeners.TryGetValue(type, out var del))
                listeners[type] = (Action<T>)del + callback;
            else
                listeners[type] = callback;
        }

        // --------------------
        // UNSUBSCRIBE
        // --------------------
        public void Unsubscribe<T>(Action<T> callback) where T : struct, IEvent
        {
            if (callback == null) return;

            var type = typeof(T);

            if (!listeners.TryGetValue(type, out var del))
                return;

            var newDel = (Action<T>)del - callback;

            if (newDel == null)
                listeners.Remove(type);
            else
                listeners[type] = newDel;
        }

        public void Dispose()
        {
            listeners.Clear();
        }
    }
}