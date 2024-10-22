using System;
using System.Collections.Generic;

public enum UIEvent
{
    //Action
    PlayGame,
    StartStage
}

public static class UIEventManager
{
    private static readonly Dictionary<UIEvent, Delegate> EventDictionary = new();

    public static void AddEvent(UIEvent uiEvent, Action action) => AddEventInternal(uiEvent, action);

    public static void AddEvent<T>(UIEvent uiEvent, Action<T> action) => AddEventInternal(uiEvent, action);

    public static void AddEvent<T1, T2>(UIEvent uiEvent, Action<T1, T2> action) =>
        AddEventInternal(uiEvent, action);

    public static void AddEvent<T1, T2, T3>(UIEvent uiEvent, Action<T1, T2, T3> action) =>
        AddEventInternal(uiEvent, action);

    private static void AddEventInternal(UIEvent uiEvent, Delegate action)
    {
        if (EventDictionary.TryAdd(uiEvent, action)) return;
        EventDictionary[uiEvent] = Delegate.Combine(EventDictionary[uiEvent], action);
    }

    public static void RemoveEvent(UIEvent uiEvent, Action action) => RemoveEventInternal(uiEvent, action);

    public static void RemoveEvent<T>(UIEvent uiEvent, Action<T> action) =>
        RemoveEventInternal(uiEvent, action);

    public static void RemoveEvent<T1, T2>(UIEvent uiEvent, Action<T1, T2> action) =>
        RemoveEventInternal(uiEvent, action);

    public static void RemoveEvent<T1, T2, T3>(UIEvent uiEvent, Action<T1, T2, T3> action) =>
        RemoveEventInternal(uiEvent, action);

    private static void RemoveEventInternal(UIEvent uiEvent, Delegate action)
    {
        if (EventDictionary.ContainsKey(uiEvent))
        {
            EventDictionary[uiEvent] = Delegate.Remove(EventDictionary[uiEvent], action);
        }
    }

    public static void TriggerEvent(UIEvent uiEvent)
    {
        if (EventDictionary.TryGetValue(uiEvent, out var action))
        {
            (action as Action)?.Invoke();
        }
    }

    public static void TriggerEvent<T>(UIEvent uiEvent, T arg)
    {
        if (EventDictionary.TryGetValue(uiEvent, out var action))
        {
            (action as Action<T>)?.Invoke(arg);
        }
    }

    public static void TriggerEvent<T1, T2>(UIEvent uiEvent, T1 arg1, T2 arg2)
    {
        if (EventDictionary.TryGetValue(uiEvent, out var action))
        {
            (action as Action<T1, T2>)?.Invoke(arg1, arg2);
        }
    }

    public static void TriggerEvent<T1, T2, T3>(UIEvent uiEvent, T1 arg1, T2 arg2, T3 arg3)
    {
        if (EventDictionary.TryGetValue(uiEvent, out var action))
        {
            (action as Action<T1, T2, T3>)?.Invoke(arg1, arg2, arg3);
        }
    }
}

public static class UIFuncManager
{
    private static readonly Dictionary<UIEvent, Delegate> FuncDictionary = new();

    public static void AddEvent<T>(UIEvent uiEvent, Func<T> func)
    {
        if (FuncDictionary.TryAdd(uiEvent, func)) return;
        FuncDictionary[uiEvent] = Delegate.Combine(FuncDictionary[uiEvent], func);
    }

    public static void RemoveEvent<T>(UIEvent uiEvent, Func<T> func)
    {
        if (FuncDictionary.ContainsKey(uiEvent))
        {
            FuncDictionary[uiEvent] = Delegate.Remove(FuncDictionary[uiEvent], func);
        }
    }

    public static T TriggerEvent<T>(UIEvent uiEvent)
    {
        if (FuncDictionary.TryGetValue(uiEvent, out var func))
        {
            if (func is Func<T> typedFunc)
            {
                return typedFunc();
            }
        }

        return default;
    }
}