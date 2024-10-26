// UniTask 전용 이벤트 enum

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public enum UniTaskEvent
{
    // Stage 관련
    CreateStage,
    ElevateBubbleContainer,
    PopBubbles,
}

public static class UniTaskEventManager
{
    private static readonly Dictionary<UniTaskEvent, Delegate> EventDictionary = new();

    // void UniTask 등록
    public static void AddEvent(UniTaskEvent actionEvent, Func<UniTask> action) =>
        AddEventInternal(actionEvent, action);

    public static void AddEvent<T>(UniTaskEvent actionEvent, Func<T, UniTask> action) =>
        AddEventInternal(actionEvent, action);

    public static void AddEvent<T1, T2>(UniTaskEvent actionEvent, Func<T1, T2, UniTask> action) =>
        AddEventInternal(actionEvent, action);

    public static void AddEvent<T1, T2, T3>(UniTaskEvent actionEvent, Func<T1, T2, T3, UniTask> action) =>
        AddEventInternal(actionEvent, action);

    // UniTask<TResult> 등록
    public static void AddEvent<TResult>(UniTaskEvent actionEvent, Func<UniTask<TResult>> func) =>
        AddEventInternal(actionEvent, func);

    public static void AddEvent<T, TResult>(UniTaskEvent actionEvent, Func<T, UniTask<TResult>> func) =>
        AddEventInternal(actionEvent, func);

    public static void AddEvent<T1, T2, TResult>(UniTaskEvent actionEvent, Func<T1, T2, UniTask<TResult>> func) =>
        AddEventInternal(actionEvent, func);

    public static void
        AddEvent<T1, T2, T3, TResult>(UniTaskEvent actionEvent, Func<T1, T2, T3, UniTask<TResult>> func) =>
        AddEventInternal(actionEvent, func);

    private static void AddEventInternal(UniTaskEvent actionEvent, Delegate action)
    {
        if (EventDictionary.TryAdd(actionEvent, action)) return;
        EventDictionary[actionEvent] = Delegate.Combine(EventDictionary[actionEvent], action);
    }

    // void UniTask 제거
    public static void RemoveEvent(UniTaskEvent actionEvent, Func<UniTask> action) =>
        RemoveEventInternal(actionEvent, action);

    public static void RemoveEvent<T>(UniTaskEvent actionEvent, Func<T, UniTask> action) =>
        RemoveEventInternal(actionEvent, action);

    public static void RemoveEvent<T1, T2>(UniTaskEvent actionEvent, Func<T1, T2, UniTask> action) =>
        RemoveEventInternal(actionEvent, action);

    public static void RemoveEvent<T1, T2, T3>(UniTaskEvent actionEvent, Func<T1, T2, T3, UniTask> action) =>
        RemoveEventInternal(actionEvent, action);

    // UniTask<TResult> 제거
    public static void RemoveEvent<TResult>(UniTaskEvent actionEvent, Func<UniTask<TResult>> func) =>
        RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T, TResult>(UniTaskEvent actionEvent, Func<T, UniTask<TResult>> func) =>
        RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T1, T2, TResult>(UniTaskEvent actionEvent, Func<T1, T2, UniTask<TResult>> func) =>
        RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T1, T2, T3, TResult>(UniTaskEvent actionEvent,
                                                        Func<T1, T2, T3, UniTask<TResult>> func) =>
        RemoveEventInternal(actionEvent, func);

    private static void RemoveEventInternal(UniTaskEvent actionEvent, Delegate action)
    {
        if (EventDictionary.ContainsKey(actionEvent))
        {
            EventDictionary[actionEvent] = Delegate.Remove(EventDictionary[actionEvent], action);
        }
    }

    // void UniTask 실행
    public static async UniTask TriggerAsync(UniTaskEvent actionEvent)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var action))
        {
            if (action is Func<UniTask> typedFunc)
            {
                await typedFunc();
            }
        }
    }

    public static async UniTask TriggerAsync<T>(UniTaskEvent actionEvent, T arg)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var action))
        {
            if (action is Func<T, UniTask> typedFunc)
            {
                await typedFunc(arg);
            }
        }
    }

    public static async UniTask TriggerAsync<T1, T2>(UniTaskEvent actionEvent, T1 arg1, T2 arg2)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var action))
        {
            if (action is Func<T1, T2, UniTask> typedFunc)
            {
                await typedFunc(arg1, arg2);
            }
        }
    }

    public static async UniTask TriggerAsync<T1, T2, T3>(UniTaskEvent actionEvent, T1 arg1, T2 arg2, T3 arg3)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var action))
        {
            if (action is Func<T1, T2, T3, UniTask> typedFunc)
            {
                await typedFunc(arg1, arg2, arg3);
            }
        }
    }

    // UniTask<TResult> 실행
    public static async UniTask<TResult> TriggerAsync<TResult>(UniTaskEvent actionEvent)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<UniTask<TResult>> typedFunc)
            {
                return await typedFunc();
            }
        }

        return default;
    }

    public static async UniTask<TResult> TriggerAsync<T, TResult>(UniTaskEvent actionEvent, T arg)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<T, UniTask<TResult>> typedFunc)
            {
                return await typedFunc(arg);
            }
        }

        return default;
    }

    public static async UniTask<TResult> TriggerAsync<T1, T2, TResult>(UniTaskEvent actionEvent, T1 arg1, T2 arg2)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<T1, T2, UniTask<TResult>> typedFunc)
            {
                return await typedFunc(arg1, arg2);
            }
        }

        return default;
    }

    public static async UniTask<TResult> TriggerAsync<T1, T2, T3, TResult>(
        UniTaskEvent actionEvent, T1 arg1, T2 arg2, T3 arg3)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<T1, T2, T3, UniTask<TResult>> typedFunc)
            {
                return await typedFunc(arg1, arg2, arg3);
            }
        }

        return default;
    }
}