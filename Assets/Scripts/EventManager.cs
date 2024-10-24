using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public enum ActionEvent
{
    //Action
    //UI
    PlayGame,

    //In Game
    CreateStage,
    GetCurrentStage,
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
    private static readonly Dictionary<ActionEvent, Delegate> FuncDictionary = new();

    // 동기 메서드 등록
    public static void AddEvent<TResult>(ActionEvent actionEvent, Func<TResult> func) => AddEventInternal(actionEvent, func);
    public static void AddEvent<T, TResult>(ActionEvent actionEvent, Func<T, TResult> func) => AddEventInternal(actionEvent, func);

    public static void AddEvent<T1, T2, TResult>(ActionEvent actionEvent, Func<T1, T2, TResult> func) =>
        AddEventInternal(actionEvent, func);

    public static void AddEvent<T1, T2, T3, TResult>(ActionEvent actionEvent, Func<T1, T2, T3, TResult> func) =>
        AddEventInternal(actionEvent, func);

    // UniTask 메서드 등록
    public static void AddEvent<TResult>(ActionEvent actionEvent, Func<UniTask<TResult>> func) =>
        AddEventInternal(actionEvent, func);

    public static void AddEvent<T, TResult>(ActionEvent actionEvent, Func<T, UniTask<TResult>> func) =>
        AddEventInternal(actionEvent, func);

    public static void AddEvent<T1, T2, TResult>(ActionEvent actionEvent, Func<T1, T2, UniTask<TResult>> func) =>
        AddEventInternal(actionEvent, func);

    public static void AddEvent<T1, T2, T3, TResult>(ActionEvent actionEvent, Func<T1, T2, T3, UniTask<TResult>> func) =>
        AddEventInternal(actionEvent, func);

    private static void AddEventInternal(ActionEvent actionEvent, Delegate func)
    {
        if (FuncDictionary.TryAdd(actionEvent, func)) return;
        FuncDictionary[actionEvent] = Delegate.Combine(FuncDictionary[actionEvent], func);
    }

    // 동기 메서드 제거
    public static void RemoveEvent<TResult>(ActionEvent actionEvent, Func<TResult> func) => RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T, TResult>(ActionEvent actionEvent, Func<T, TResult> func) =>
        RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T1, T2, TResult>(ActionEvent actionEvent, Func<T1, T2, TResult> func) =>
        RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T1, T2, T3, TResult>(ActionEvent actionEvent, Func<T1, T2, T3, TResult> func) =>
        RemoveEventInternal(actionEvent, func);

    // UniTask 메서드 제거
    public static void RemoveEvent<TResult>(ActionEvent actionEvent, Func<UniTask<TResult>> func) =>
        RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T, TResult>(ActionEvent actionEvent, Func<T, UniTask<TResult>> func) =>
        RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T1, T2, TResult>(ActionEvent actionEvent, Func<T1, T2, UniTask<TResult>> func) =>
        RemoveEventInternal(actionEvent, func);

    public static void RemoveEvent<T1, T2, T3, TResult>(ActionEvent actionEvent, Func<T1, T2, T3, UniTask<TResult>> func) =>
        RemoveEventInternal(actionEvent, func);

    private static void RemoveEventInternal(ActionEvent actionEvent, Delegate func)
    {
        if (FuncDictionary.ContainsKey(actionEvent))
        {
            FuncDictionary[actionEvent] = Delegate.Remove(FuncDictionary[actionEvent], func);
        }
    }

    // 동기 메서드 실행
    public static TResult TriggerEvent<TResult>(ActionEvent actionEvent)
    {
        if (FuncDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<TResult> typedFunc)
            {
                return typedFunc();
            }
        }

        return default;
    }

    public static TResult TriggerEvent<T, TResult>(ActionEvent actionEvent, T arg)
    {
        if (FuncDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<T, TResult> typedFunc)
            {
                return typedFunc(arg);
            }
        }

        return default;
    }

    public static TResult TriggerEvent<T1, T2, TResult>(ActionEvent actionEvent, T1 arg1, T2 arg2)
    {
        if (FuncDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<T1, T2, TResult> typedFunc)
            {
                return typedFunc(arg1, arg2);
            }
        }

        return default;
    }

    public static TResult TriggerEvent<T1, T2, T3, TResult>(ActionEvent actionEvent, T1 arg1, T2 arg2, T3 arg3)
    {
        if (FuncDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<T1, T2, T3, TResult> typedFunc)
            {
                return typedFunc(arg1, arg2, arg3);
            }
        }

        return default;
    }

    // UniTask 메서드 실행
    public static async UniTask<TResult> TriggerEventAsync<TResult>(ActionEvent actionEvent)
    {
        if (FuncDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<UniTask<TResult>> typedFunc)
            {
                return await typedFunc();
            }
        }

        return default;
    }

    public static async UniTask<TResult> TriggerEventAsync<T, TResult>(ActionEvent actionEvent, T arg)
    {
        if (FuncDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<T, UniTask<TResult>> typedFunc)
            {
                return await typedFunc(arg);
            }
        }

        return default;
    }

    public static async UniTask<TResult> TriggerEventAsync<T1, T2, TResult>(ActionEvent actionEvent, T1 arg1, T2 arg2)
    {
        if (FuncDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<T1, T2, UniTask<TResult>> typedFunc)
            {
                return await typedFunc(arg1, arg2);
            }
        }

        return default;
    }

    public static async UniTask<TResult> TriggerEventAsync<T1, T2, T3, TResult>(
        ActionEvent actionEvent, T1 arg1, T2 arg2, T3 arg3)
    {
        if (FuncDictionary.TryGetValue(actionEvent, out var func))
        {
            if (func is Func<T1, T2, T3, UniTask<TResult>> typedFunc)
            {
                return await typedFunc(arg1, arg2, arg3);
            }
        }

        return default;
    }
}