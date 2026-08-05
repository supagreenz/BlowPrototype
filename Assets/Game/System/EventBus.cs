using System;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;

public static class EventBus<T> where T : struct
{
    [AutoStaticsCleanup] private static Action<T> _onEvent;
    [AutoStaticsCleanup] private static Action _onEventVacant;

    public static void Subscribe(Action<T> listener) => _onEvent += listener;

    public static void Subscribe(Action listener) => _onEventVacant += listener;

    public static void Unsubscribe(Action<T> listener) => _onEvent -= listener;
    public static void Unsubscribe(Action listener) => _onEventVacant -= listener;

    public static void Raise(T eventItem)
    {
        _onEvent?.Invoke(eventItem);
        _onEventVacant?.Invoke();
    }

    public static void Raise()
    {
        _onEventVacant?.Invoke();
    }

    public static class Keyed<TKey> where TKey : Enum
    {
        [AutoStaticsCleanup] private static readonly Dictionary<TKey, Action<T>> _handlers = new();
        [AutoStaticsCleanup] private static readonly Dictionary<TKey, Action> _handlersVacant = new();

        public static void Subscribe(TKey key, Action<T> listener)
        {
            _handlers.TryGetValue(key, out var current);
            _handlers[key] = current + listener;
        }

        public static void Subscribe(TKey key, Action listener)
        {
            _handlersVacant.TryGetValue(key, out var current);
            _handlersVacant[key] = current + listener;
        }

        public static void Unsubscribe(TKey key, Action<T> listener)
        {
            if (!_handlers.TryGetValue(key, out var current)) return;
            current -= listener;
            if (current == null) _handlers.Remove(key);
            else _handlers[key] = current;
        }

        public static void Unsubscribe(TKey key, Action listener)
        {
            if (!_handlersVacant.TryGetValue(key, out var current)) return;
            current -= listener;
            if (current == null) _handlersVacant.Remove(key);
            else _handlersVacant[key] = current;
        }

        public static void Raise(TKey key, T eventItem)
        {
            if (_handlers.TryGetValue(key, out var h)) h?.Invoke(eventItem);
            if (_handlersVacant.TryGetValue(key, out var hv)) hv?.Invoke();
            _onEvent?.Invoke(eventItem);
            _onEventVacant?.Invoke();
        }

        public static void Raise(TKey key)
        {
            if (_handlersVacant.TryGetValue(key, out var hv)) hv?.Invoke();
            _onEventVacant?.Invoke();
        }
    }
}