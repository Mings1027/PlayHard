using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public enum ActionEvent
{
    //In Game
    AddBubble,
    CheckMatchingBubble,
    PopBubble,
}

public enum FuncEvent
{
    VisibleBubbles,
}

public static class EventManager
{
    private static readonly Dictionary<ActionEvent, Delegate> EventDictionary = new();

    public static void AddEvent(ActionEvent actionEvent, Action action) => AddEventInternal(actionEvent, action);

    public static void AddEvent<T>(ActionEvent actionEvent, Action<T> action) => AddEventInternal(actionEvent, action);

    public static void AddEvent<T1, T2>(ActionEvent actionEvent, Action<T1, T2> action) =>
        AddEventInternal(actionEvent, action);

    public static void AddEvent<T1, T2, T3>(ActionEvent actionEvent, Action<T1, T2, T3> action) =>
        AddEventInternal(actionEvent, action);

    private static void AddEventInternal(ActionEvent actionEvent, Delegate action)
    {
        if (EventDictionary.TryAdd(actionEvent, action)) return;
        EventDictionary[actionEvent] = Delegate.Combine(EventDictionary[actionEvent], action);
    }

    public static void RemoveEvent(ActionEvent actionEvent, Action action) => RemoveEventInternal(actionEvent, action);

    public static void RemoveEvent<T>(ActionEvent actionEvent, Action<T> action) =>
        RemoveEventInternal(actionEvent, action);

    public static void RemoveEvent<T1, T2>(ActionEvent actionEvent, Action<T1, T2> action) =>
        RemoveEventInternal(actionEvent, action);

    public static void RemoveEvent<T1, T2, T3>(ActionEvent actionEvent, Action<T1, T2, T3> action) =>
        RemoveEventInternal(actionEvent, action);

    private static void RemoveEventInternal(ActionEvent actionEvent, Delegate action)
    {
        if (EventDictionary.ContainsKey(actionEvent))
        {
            EventDictionary[actionEvent] = Delegate.Remove(EventDictionary[actionEvent], action);
        }
    }

    public static void TriggerEvent(ActionEvent actionEvent)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var action))
        {
            (action as Action)?.Invoke();
        }
    }

    public static void TriggerEvent<T>(ActionEvent actionEvent, T arg)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var action))
        {
            (action as Action<T>)?.Invoke(arg);
        }
    }

    public static void TriggerEvent<T1, T2>(ActionEvent actionEvent, T1 arg1, T2 arg2)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var action))
        {
            (action as Action<T1, T2>)?.Invoke(arg1, arg2);
        }
    }

    public static void TriggerEvent<T1, T2, T3>(ActionEvent actionEvent, T1 arg1, T2 arg2, T3 arg3)
    {
        if (EventDictionary.TryGetValue(actionEvent, out var action))
        {
            (action as Action<T1, T2, T3>)?.Invoke(arg1, arg2, arg3);
        }
    }
}

public static class FuncManager
{
    private static readonly Dictionary<FuncEvent, Delegate> FuncDictionary = new();

    // 동기 메서드 등록
    public static void AddEvent<TResult>(FuncEvent actionEvent, Func<TResult> func) =>
        AddEventInternal(actionEvent, func);

    public static void AddEvent<T, TResult>(FuncEvent actionEvent, Func<T, TResult> func) =>
        AddEventInternal(actionEvent, func);

    public static void AddEvent<T1, T2, TResult>(FuncEvent actionEvent, Func<T1, T2, TResult> func) =>
        AddEventInternal(actionEvent, func);

    public static void AddEvent<T1, T2, T3, TResult>(FuncEvent actionEvent, Func<T1, T2, T3, TResult> func) =>
        AddEventInternal(actionEvent, func);


    // UniTask 메서드 등록
    public static void AddEvent(FuncEvent funcEvent, Func<UniTask> action) => AddEventInternal(funcEvent, action);
    public static void AddEvent<T>(FuncEvent funcEvent, Func<T, UniTask> action) => AddEventInternal(funcEvent, action);

    public static void AddEvent<T1, T2>(FuncEvent funcEvent, Func<T1, T2, UniTask> action) =>
        AddEventInternal(funcEvent, action);

    public static void AddEvent<T1, T2, T3>(FuncEvent funcEvent, Func<T1, T2, T3, UniTask> action) =>
        AddEventInternal(funcEvent, action);

    public static void AddEvent<TResult>(FuncEvent funcEvent, Func<UniTask<TResult>> action) =>
        AddEventInternal(funcEvent, action);

    public static void AddEvent<T, TResult>(FuncEvent funcEvent, Func<T, UniTask<TResult>> func) =>
        AddEventInternal(funcEvent, func);

    public static void AddEvent<T1, T2, TResult>(FuncEvent funcEvent, Func<T1, T2, UniTask<TResult>> func) =>
        AddEventInternal(funcEvent, func);

    public static void AddEvent<T1, T2, T3, TResult>(FuncEvent funcEvent, Func<T1, T2, T3, UniTask<TResult>> func) =>
        AddEventInternal(funcEvent, func);


    private static void AddEventInternal(FuncEvent actionEvent, Delegate func)
    {
        if (FuncDictionary.TryAdd(actionEvent, func)) return;
        FuncDictionary[actionEvent] = Delegate.Combine(FuncDictionary[actionEvent], func);
    }

    // 동기 메서드 제거
    public static void RemoveEvent<TResult>(FuncEvent actionEvent, Func<TResult> func) =>
        RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T, TResult>(FuncEvent actionEvent, Func<T, TResult> func) =>
        RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T1, T2, TResult>(FuncEvent actionEvent, Func<T1, T2, TResult> func) =>
        RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T1, T2, T3, TResult>(FuncEvent actionEvent, Func<T1, T2, T3, TResult> func) =>
        RemoveEventInternal(actionEvent, func);


    // UniTask 메서드 제거
    public static void RemoveEvent(FuncEvent funcEvent, Func<UniTask> action) => RemoveEventInternal(funcEvent, action);

    public static void RemoveEvent<T>(FuncEvent funcEvent, Func<T, UniTask> func) =>
        RemoveEventInternal(funcEvent, func);

    public static void RemoveEvent<T1, T2>(FuncEvent funcEvent, Func<T1, T2, UniTask> func) =>
        RemoveEventInternal(funcEvent, func);

    public static void RemoveEvent<T1, T2, T3>(FuncEvent funcEvent, Func<T1, T2, T3, UniTask> func) =>
        RemoveEventInternal(funcEvent, func);


    public static void RemoveEvent<TResult>(FuncEvent funcEvent, Func<UniTask<TResult>> func) =>
        RemoveEventInternal(funcEvent, func);

    public static void RemoveEvent<T, TResult>(FuncEvent funcEvent, Func<T, UniTask<TResult>> func) =>
        RemoveEventInternal(funcEvent, func);

    public static void RemoveEvent<T1, T2, TResult>(FuncEvent funcEvent, Func<T1, T2, UniTask<TResult>> func) =>
        RemoveEventInternal(funcEvent, func);

    public static void RemoveEvent<T1, T2, T3, TResult>(FuncEvent funcEvent,
                                                        Func<T1, T2, T3, UniTask<TResult>> func) =>
        RemoveEventInternal(funcEvent, func);

    private static void RemoveEventInternal(FuncEvent funcEvent, Delegate func)
    {
        if (FuncDictionary.ContainsKey(funcEvent))
        {
            FuncDictionary[funcEvent] = Delegate.Remove(FuncDictionary[funcEvent], func);
        }
    }

    // 동기 메서드 실행
    public static TResult TriggerEvent<TResult>(FuncEvent funcEvent)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<TResult> typedFunc)
            {
                return typedFunc();
            }
        }

        return default;
    }

    public static TResult TriggerEvent<T, TResult>(FuncEvent funcEvent, T arg)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<T, TResult> typedFunc)
            {
                return typedFunc(arg);
            }
        }

        return default;
    }

    public static TResult TriggerEvent<T1, T2, TResult>(FuncEvent funcEvent, T1 arg1, T2 arg2)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<T1, T2, TResult> typedFunc)
            {
                return typedFunc(arg1, arg2);
            }
        }

        return default;
    }

    public static TResult TriggerEvent<T1, T2, T3, TResult>(FuncEvent funcEvent, T1 arg1, T2 arg2, T3 arg3)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<T1, T2, T3, TResult> typedFunc)
            {
                return typedFunc(arg1, arg2, arg3);
            }
        }

        return default;
    }

    // void 반환하는 UniTask
    public static async UniTask TriggerEventAsync(FuncEvent funcEvent)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<UniTask> typedFunc)
            {
                await typedFunc();
            }
        }
    }

    public static async UniTask TriggerEventAsync<T>(FuncEvent funcEvent, T arg)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<T, UniTask> typedFunc)
            {
                await typedFunc(arg);
            }
        }
    }

    public static async UniTask TriggerEventAsync<T1, T2>(FuncEvent funcEvent, T1 arg1, T2 arg2)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<T1, T2, UniTask> typedFunc)
            {
                await typedFunc(arg1, arg2);
            }
        }
    }

    public static async UniTask TriggerEventAsync<T1, T2, T3>(FuncEvent funcEvent, T1 arg1, T2 arg2, T3 arg3)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<T1, T2, T3, UniTask> typedFunc)
            {
                await typedFunc(arg1, arg2, arg3);
            }
        }
    }

    //값 반환하는 UniTask 실행
    public static async UniTask<TResult> TriggerEventAsync<TResult>(FuncEvent funcEvent)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<UniTask<TResult>> typedFunc)
            {
                return await typedFunc();
            }
        }

        return default;
    }

    public static async UniTask<TResult> TriggerEventAsync<T, TResult>(FuncEvent funcEvent, T arg)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<T, UniTask<TResult>> typedFunc)
            {
                return await typedFunc(arg);
            }
        }

        return default;
    }

    public static async UniTask<TResult> TriggerEventAsync<T1, T2, TResult>(FuncEvent funcEvent, T1 arg1, T2 arg2)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<T1, T2, UniTask<TResult>> typedFunc)
            {
                return await typedFunc(arg1, arg2);
            }
        }

        return default;
    }

    public static async UniTask<TResult> TriggerEventAsync<T1, T2, T3, TResult>(
        FuncEvent funcEvent, T1 arg1, T2 arg2, T3 arg3)
    {
        if (FuncDictionary.TryGetValue(funcEvent, out var func))
        {
            if (func is Func<T1, T2, T3, UniTask<TResult>> typedFunc)
            {
                return await typedFunc(arg1, arg2, arg3);
            }
        }

        return default;
    }
}