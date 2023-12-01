using System;
using System.Collections.Generic;

namespace NR_AutoMachineTool.Utilities;

public class Option<T>
{
    public Option(T val)
    {
        if (val == null)
        {
            return;
        }

        Value = val;
        HasValue = true;
    }

    public Option()
    {
    }

    public T Value { get; }

    public bool HasValue { get; }

    public Option<TO> SelectMany<TO>(Func<T, Option<TO>> func)
    {
        return !HasValue ? new Nothing<TO>() : func(Value);
    }

    public Option<TO> Select<TO>(Func<T, TO> func)
    {
        return !HasValue ? new Nothing<TO>() : new Option<TO>(func(Value));
    }

    public Option<T> Where(Predicate<T> pre)
    {
        if (!HasValue)
        {
            return new Nothing<T>();
        }

        return !pre(Value) ? new Nothing<T>() : this;
    }

    public void ForEach(Action<T> act)
    {
        if (HasValue)
        {
            act(Value);
        }
    }

    public List<T> ToList()
    {
        return !HasValue ? [] : [..new[] { Value }];
    }

    public T GetOrDefault(T defaultValue)
    {
        return !HasValue ? defaultValue : Value;
    }

    public T GetOrDefaultF(Func<T> creator)
    {
        return !HasValue ? creator() : Value;
    }

    public Func<Func<T, R>, R> Fold<R>(R defaultValue)
    {
        if (!HasValue)
        {
            return _ => defaultValue;
        }

        return f => f(Value);
    }

    public Func<Func<T, R>, R> Fold<R>(Func<R> craetor)
    {
        if (!HasValue)
        {
            return _ => craetor();
        }

        return f => f(Value);
    }

    public Option<T> Peek(Action<T> act)
    {
        if (HasValue)
        {
            act(Value);
        }

        return this;
    }

    public override bool Equals(object obj)
    {
        if (obj is not Option<T> option || HasValue != option.HasValue)
        {
            return false;
        }

        if (!HasValue)
        {
            return true;
        }

        var val = Value;
        return val.Equals(option.Value);
    }

    public override int GetHashCode()
    {
        if (!HasValue)
        {
            return HasValue.GetHashCode();
        }

        var val = Value;
        return val.GetHashCode();
    }

    public override string ToString()
    {
        return Fold("Option<Nothing>")(v => "Option<" + v + ">");
    }
}