using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace EventControl
{
    public enum BubbleEvent
    {
        //Action
        PopMatchingBubbles,
        CheckMatchingBubble,
        AddBubble,
        PopBubble,


        //Func
        CreateBubble,
        RandomShooterBubble,
        RandomStageBubble,
        CreatePreviewBubble
    }

    public static class BubbleEventManager
    {
        private static readonly Dictionary<BubbleEvent, Delegate> EventDictionary = new();

        // 반환값이 없는 이벤트 등록
        public static void AddEvent(BubbleEvent bubbleEvent, Action action) =>
            AddEventInternal(bubbleEvent, action);

        public static void AddEvent<T>(BubbleEvent bubbleEvent, Action<T> action) =>
            AddEventInternal(bubbleEvent, action);

        public static void AddEvent<T1, T2>(BubbleEvent bubbleEvent, Action<T1, T2> action) =>
            AddEventInternal(bubbleEvent, action);

        public static void AddEvent<T1, T2, T3>(BubbleEvent bubbleEvent, Action<T1, T2, T3> action) =>
            AddEventInternal(bubbleEvent, action);

        // 반환값이 있는 이벤트 등록
        public static void AddEvent<TResult>(BubbleEvent bubbleEvent, Func<TResult> func) =>
            AddEventInternal(bubbleEvent, func);

        public static void AddEvent<T, TResult>(BubbleEvent bubbleEvent, Func<T, TResult> func) =>
            AddEventInternal(bubbleEvent, func);

        public static void AddEvent<T1, T2, TResult>(BubbleEvent bubbleEvent, Func<T1, T2, TResult> func) =>
            AddEventInternal(bubbleEvent, func);

        public static void AddEvent<T1, T2, T3, TResult>(BubbleEvent bubbleEvent, Func<T1, T2, T3, TResult> func) =>
            AddEventInternal(bubbleEvent, func);

        // UniTask 지원
        public static void AddAsyncEvent<T>(BubbleEvent bubbleEvent, Func<T, UniTask> func) =>
            AddEventInternal(bubbleEvent, func);

        public static void AddAsyncEvent<TResult>(BubbleEvent bubbleEvent, Func<UniTask<TResult>> func) =>
            AddEventInternal(bubbleEvent, func);

        public static void AddAsyncEvent<T, TResult>(BubbleEvent bubbleEvent, Func<T, UniTask<TResult>> func) =>
            AddEventInternal(bubbleEvent, func);

        private static void AddEventInternal(BubbleEvent bubbleEvent, Delegate eventDelegate)
        {
            if (EventDictionary.TryAdd(bubbleEvent, eventDelegate)) return;
            EventDictionary[bubbleEvent] = Delegate.Combine(EventDictionary[bubbleEvent], eventDelegate);
        }

        // 이벤트 제거 메서드들
        public static void RemoveEvent(BubbleEvent bubbleEvent, Action action)
        {
            RemoveEventInternal(bubbleEvent, action);
        }

        public static void RemoveEvent<T>(BubbleEvent bubbleEvent, Action<T> action)
        {
            RemoveEventInternal(bubbleEvent, action);
        }

        public static void RemoveEvent<T1, T2>(BubbleEvent bubbleEvent, Action<T1, T2> action) =>
            RemoveEventInternal(bubbleEvent, action);

        public static void RemoveEvent<T1, T2, T3>(BubbleEvent bubbleEvent, Action<T1, T2, T3> action) =>
            RemoveEventInternal(bubbleEvent, action);

        public static void RemoveEvent<TResult>(BubbleEvent bubbleEvent, Func<TResult> func) =>
            RemoveEventInternal(bubbleEvent, func);

        public static void RemoveEvent<T, TResult>(BubbleEvent bubbleEvent, Func<T, TResult> func) =>
            RemoveEventInternal(bubbleEvent, func);

        public static void RemoveEvent<T1, T2, TResult>(BubbleEvent bubbleEvent, Func<T1, T2, TResult> func) =>
            RemoveEventInternal(bubbleEvent, func);

        public static void RemoveEvent<T1, T2, T3, TResult>(BubbleEvent bubbleEvent, Func<T1, T2, T3, TResult> func) =>
            RemoveEventInternal(bubbleEvent, func);

        public static void RemoveAsyncEvent<T>(BubbleEvent bubbleEvent, Func<T, UniTask> func) =>
            RemoveEventInternal(bubbleEvent, func);

        private static void RemoveEventInternal(BubbleEvent bubbleEvent, Delegate eventDelegate)
        {
            if (EventDictionary.ContainsKey(bubbleEvent))
            {
                EventDictionary[bubbleEvent] = Delegate.Remove(EventDictionary[bubbleEvent], eventDelegate);
            }
        }

        // 이벤트 트리거 메서드들 (반환값 없음)
        public static void TriggerEvent(BubbleEvent bubbleEvent)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var action))
            {
                (action as Action)?.Invoke();
            }
        }

        public static void TriggerEvent<T>(BubbleEvent bubbleEvent, T arg)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var action))
            {
                (action as Action<T>)?.Invoke(arg);
            }
        }

        public static void TriggerEvent<T1, T2>(BubbleEvent bubbleEvent, T1 arg1, T2 arg2)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var action))
            {
                (action as Action<T1, T2>)?.Invoke(arg1, arg2);
            }
        }

        public static void TriggerEvent<T1, T2, T3>(BubbleEvent bubbleEvent, T1 arg1, T2 arg2, T3 arg3)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var action))
            {
                (action as Action<T1, T2, T3>)?.Invoke(arg1, arg2, arg3);
            }
        }

        // 반환값이 있는 이벤트 트리거
        public static TResult TriggerEvent<TResult>(BubbleEvent bubbleEvent)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var func))
            {
                if (func is Func<TResult> typedFunc)
                {
                    return typedFunc();
                }
            }

            return default;
        }

        public static TResult TriggerEvent<T, TResult>(BubbleEvent bubbleEvent, T arg)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var func))
            {
                if (func is Func<T, TResult> typedFunc)
                {
                    return typedFunc(arg);
                }
            }

            return default;
        }

        public static TResult TriggerEvent<T1, T2, TResult>(BubbleEvent bubbleEvent, T1 arg1, T2 arg2)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var func))
            {
                if (func is Func<T1, T2, TResult> typedFunc)
                {
                    return typedFunc(arg1, arg2);
                }
            }

            return default;
        }

        public static TResult TriggerEvent<T1, T2, T3, TResult>(
            BubbleEvent bubbleEvent, T1 arg1, T2 arg2, T3 arg3)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var func))
            {
                if (func is Func<T1, T2, T3, TResult> typedFunc)
                {
                    return typedFunc(arg1, arg2, arg3);
                }
            }

            return default;
        }

        public static async UniTask TriggerAsync(BubbleEvent bubbleEvent)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var func))
            {
                if (func is Func<UniTask> typedFunc)
                {
                    await typedFunc();
                }
            }
        }

        public static async UniTask TriggerAsync<T>(BubbleEvent bubbleEvent, T arg)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var func))
            {
                if (func is Func<T, UniTask> typedFunc)
                {
                    await typedFunc(arg);
                }
            }
        }

        // UniTask 지원 트리거
        public static async UniTask<TResult> TriggerEventAsync<TResult>(BubbleEvent bubbleEvent)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var func))
            {
                if (func is Func<UniTask<TResult>> asyncFunc)
                {
                    return await asyncFunc();
                }
            }

            return default;
        }

        public static async UniTask<TResult> TriggerEventAsync<T, TResult>(BubbleEvent bubbleEvent, T arg)
        {
            if (EventDictionary.TryGetValue(bubbleEvent, out var func))
            {
                if (func is Func<T, UniTask<TResult>> asyncFunc)
                {
                    return await asyncFunc(arg);
                }
            }

            return default;
        }
    }
}