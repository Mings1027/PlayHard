using System;
using System.Collections.Generic;

namespace EventControl
{
    public enum SoundEvent
    {
        PopNormalBubble,
        PopBombBubble,
    }

    public enum SoundFunc
    {
        
    }

    public static class SoundEventManager
    {
        private static readonly Dictionary<SoundEvent, Delegate> EventDictionary = new();

        public static void AddEvent(SoundEvent soundEvent, Action action)
        {
            AddEventInternal(soundEvent, action);
        }

        public static void AddEvent<T>(SoundEvent soundEvent, Action<T> action)
        {
            AddEventInternal(soundEvent, action);
        }

        public static void AddEvent<T1, T2>(SoundEvent soundEvent, Action<T1, T2> action)
        {
            AddEventInternal(soundEvent, action);
        }

        private static void AddEventInternal(SoundEvent soundEvent, Delegate action)
        {
            if (!EventDictionary.ContainsKey(soundEvent))
            {
                EventDictionary[soundEvent] = action;
            }
            else
            {
                EventDictionary[soundEvent] = Delegate.Combine(EventDictionary[soundEvent], action);
            }
        }

        public static void RemoveEvent(SoundEvent soundEvent, Action action)
        {
            RemoveEventInternal(soundEvent, action);
        }

        public static void RemoveEvent<T>(SoundEvent soundEvent, Action<T> action)
        {
            RemoveEventInternal(soundEvent, action);
        }

        public static void RemoveEvent<T1, T2>(SoundEvent soundEvent, Action<T1, T2> action)
        {
            RemoveEventInternal(soundEvent, action);
        }

        private static void RemoveEventInternal(SoundEvent soundEvent, Delegate action)
        {
            if (EventDictionary.ContainsKey(soundEvent))
            {
                EventDictionary[soundEvent] = Delegate.Remove(EventDictionary[soundEvent], action);
            }
        }

        public static void TriggerEvent(SoundEvent soundEvent)
        {
            if (EventDictionary.TryGetValue(soundEvent, out var action))
            {
                (action as Action)?.Invoke();
            }
        }

        public static void TriggerEvent<T>(SoundEvent soundEvent, T arg)
        {
            if (EventDictionary.TryGetValue(soundEvent, out var action))
            {
                (action as Action<T>)?.Invoke(arg);
            }
        }

        public static void TriggerEvent<T1, T2>(SoundEvent soundEvent, T1 arg1, T2 arg2)
        {
            if (EventDictionary.TryGetValue(soundEvent, out var action))
            {
                (action as Action<T1, T2>)?.Invoke(arg1, arg2);
            }
        }
    }

    public static class SoundFuncManager
    {
        private static readonly Dictionary<SoundFunc, Delegate> FuncDictionary = new();

        public static void AddEvent<T>(SoundFunc soundFunc, Func<T> func)
        {
            if (FuncDictionary.TryAdd(soundFunc, func)) return;
            FuncDictionary[soundFunc] = Delegate.Combine(FuncDictionary[soundFunc], func);
        }

        public static void RemoveEvent<T>(SoundFunc soundFunc, Func<T> func)
        {
            if (FuncDictionary.ContainsKey(soundFunc))
            {
                FuncDictionary[soundFunc] = Delegate.Remove(FuncDictionary[soundFunc], func);
            }
        }

        public static T TriggerEvent<T>(SoundFunc soundFunc)
        {
            if (FuncDictionary.TryGetValue(soundFunc, out var func))
            {
                if (func is Func<T> typedFunc)
                {
                    return typedFunc();
                }
            }

            return default;
        }
    }
}