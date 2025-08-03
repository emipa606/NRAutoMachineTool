using System;
using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using Verse;

namespace NR_AutoMachineTool;

public class ThingLister(Map map)
{
    private readonly Dictionary<Type, List<ThingDef>> typeDic = new();

    public IEnumerable<T> ForAssignableFrom<T>() where T : Thing
    {
        if (typeDic.TryGetValue(typeof(T), out var value))
        {
            return value.SelectMany(d => map.listerThings.ThingsOfDef(d)).SelectMany(t => Ops.Option(t as T));
        }

        value = DefDatabase<ThingDef>.AllDefs.Where(d => typeof(T).IsAssignableFrom(d.thingClass)).ToList();
        typeDic[typeof(T)] = value;

        return value.SelectMany(d => map.listerThings.ThingsOfDef(d)).SelectMany(t => Ops.Option(t as T));
    }
}