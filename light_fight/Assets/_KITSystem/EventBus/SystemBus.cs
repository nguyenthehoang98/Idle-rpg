using System;
using System.Collections.Generic;

namespace _KITSystem.EventBus
{
    public sealed class SystemBus
    {
        static SystemBus instance;

        // Mỗi event type giữ đúng Action<T>
        private readonly Dictionary<Type, Delegate> listeners;

        public SystemBus()
        {
            listeners = new Dictionary<Type, Delegate>(32);
        }

        // --------------------
        // PUBLISH
        // --------------------
        private void _Publish<T>(T evt) where T : struct, ISignal
        {
            if (listeners.TryGetValue(typeof(T), out var del))
            {
                ((Action<T>)del)?.Invoke(evt);
            }
        }

        // --------------------
        // SUBSCRIBE
        // --------------------
        private void _Subscribe<T>(Action<T> callback) where T : struct, ISignal
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
        private void _Unsubscribe<T>(Action<T> callback) where T : struct, ISignal
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

        #region Static

        public static void Init() => instance = new SystemBus();

        public static void Publish<T>(T evt) where T : struct, ISignal => instance._Publish(evt);

        public static void Subscribe<T>(Action<T> callback) where T : struct, ISignal => instance._Subscribe(callback);
        
        public static void Unsubscribe<T>(Action<T> callback) where T : struct, ISignal => instance._Unsubscribe(callback);

        #endregion
    }
}