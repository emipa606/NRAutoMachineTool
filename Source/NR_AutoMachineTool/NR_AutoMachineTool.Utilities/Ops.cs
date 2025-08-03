using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NR_AutoMachineTool.Utilities;

public static class Ops
{
    public static Option<T> Option<T>(T value)
    {
        return new Option<T>(value);
    }

    public static Nothing<T> Nothing<T>()
    {
        return new Nothing<T>();
    }

    public static Just<T> Just<T>(T value)
    {
        return new Just<T>(value);
    }

    public static Option<T> FirstOption<T>(this IEnumerable<T> e)
    {
        using var enumerator = e.GetEnumerator();
        if (enumerator.MoveNext())
        {
            return new Just<T>(enumerator.Current);
        }

        return new Nothing<T>();
    }

    public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source,
        Func<TSource, Option<TResult>> selector)
    {
        return source.SelectMany(e => selector(e).ToList());
    }

    public static List<T> Append<T>(this List<T> lhs, List<T> rhs)
    {
        lhs.AddRange(rhs);
        return lhs;
    }

    public static List<T> Append<T>(this List<T> lhs, T rhs)
    {
        lhs.Add(rhs);
        return lhs;
    }

    public static List<T> Ins<T>(this List<T> lhs, int index, T rhs)
    {
        lhs.Insert(index, rhs);
        return lhs;
    }

    public static List<T> Head<T>(this List<T> lhs, T rhs)
    {
        lhs.Insert(0, rhs);
        return lhs;
    }

    public static List<T> Del<T>(this List<T> lhs, T rhs)
    {
        lhs.Remove(rhs);
        return lhs;
    }

    public static Option<T> ElementAtOption<T>(this List<T> list, int index)
    {
        return index >= list.Count ? new Nothing<T>() : Option(list[index]);
    }

    public static bool EqualValues<T>(this IEnumerable<T> lhs, IEnumerable<T> rhs)
    {
        var list = lhs.ToList();
        var list2 = rhs.ToList();
        if (list.Count != list2.Count)
        {
            return false;
        }

        for (var i = 0; i < list.Count; i++)
        {
            if (!list[i].Equals(list2[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
    {
        foreach (var item in sequence)
        {
            action(item);
        }
    }

    public static IEnumerable<T> Peek<T>(this IEnumerable<T> sequence, Action<T> action)
    {
        foreach (var item in sequence)
        {
            action(item);
            yield return item;
        }
    }

    public static Option<T> FindOption<T>(this List<T> sequence, Predicate<T> predicate)
    {
        var num = sequence.FindIndex(predicate);
        if (num == -1)
        {
            return new Nothing<T>();
        }

        return new Just<T>(sequence[num]);
    }

    public static Option<V> GetOption<K, V>(this Dictionary<K, V> dict, K key)
    {
        if (dict.TryGetValue(key, out var value))
        {
            return Just(value);
        }

        return Nothing<V>();
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
    {
        return new HashSet<T>(source, comparer);
    }

    public static Tuple<T1, T2> Tuple<T1, T2>(T1 v1, T2 v2)
    {
        return new Tuple<T1, T2>(v1, v2);
    }

    public static Tuple<T1, T2, T3> Tuple<T1, T2, T3>(T1 v1, T2 v2, T3 v3)
    {
        return new Tuple<T1, T2, T3>(v1, v2, v3);
    }

    public static IEnumerable<IEnumerable<T>> Grouped<T>(this IEnumerable<T> source, int size)
    {
        while (source.Any())
        {
            yield return source.Take(size);
            source = source.Skip(size);
        }
    }

    public static List<List<T>> Grouped<T>(this List<T> source, int size)
    {
        var list = new List<List<T>>();
        int num;
        for (var i = 0; i < source.Count; i += num)
        {
            num = Mathf.Min(size, source.Count - i);
            list.Add(source.GetRange(i, num));
        }

        return list;
    }

    public static IEnumerable<T> GetEnumValues<T>() where T : struct
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    public static bool PlaceItem(Thing t, IntVec3 cell, bool forbid, Map map, bool firstAbsorbStack = false)
    {
        if (firstAbsorbStack && Func())
        {
            return true;
        }

        if (!(from ti in cell.GetThingList(map)
                where ti.def.category == ThingCategory.Item
                select ti).Any())
        {
            GenPlace.TryPlaceThing(t, cell, map, ThingPlaceMode.Direct);
            if (forbid)
            {
                t.SetForbidden(true);
            }

            Effect(t);
            return true;
        }

        if (!firstAbsorbStack && Func())
        {
            return true;
        }

        var option = (from c in cell.SlotGroupCells(map)
            where c.IsValidStorageFor(map, t)
            where (from b in c.GetThingList(map)
                where b.def.category == ThingCategory.Building
                select b).All(b => b is not Building_BeltConveyor)
            select c).FirstOption();
        if (!option.HasValue)
        {
            return false;
        }

        GenPlace.TryPlaceThing(t, option.Value, map, ThingPlaceMode.Near);
        if (forbid)
        {
            t.SetForbidden(true);
        }

        Effect(t);
        return true;

        void Effect(Thing item)
        {
            item.def.soundDrop.PlayOneShot(item);
            FleckMaker.ThrowDustPuff(item.Position, map, 0.5f);
        }

        bool Func()
        {
            (from i in cell.SlotGroupCells(map).SelectMany(c => c.GetThingList(map)) where i.def == t.def select i)
                .ForEach(delegate(Thing i) { i.TryAbsorbStack(t, true); });
            if (t.stackCount != 0)
            {
                return false;
            }

            Effect(t);
            return true;
        }
    }

    public static void Noop()
    {
    }

    public static List<IntVec3> SlotGroupCells(this IntVec3 c, Map map)
    {
        return (from g in Option(map.haulDestinationManager.SlotGroupAt(c))
            select g.CellsList).GetOrDefault(new List<IntVec3>().Append(c));
    }

    public static TO CopyTo<FROM, TO>(this FROM bill, TO copy) where FROM : Bill_Production where TO : Bill_Production
    {
        copy.allowedSkillRange = bill.allowedSkillRange;
        copy.billStack = bill.billStack;
        copy.deleted = bill.deleted;
        copy.hpRange = bill.hpRange;
        copy.includeEquipped = bill.includeEquipped;
        copy.includeGroup = bill.includeGroup;
        copy.includeTainted = bill.includeTainted;
        copy.ingredientFilter = bill.ingredientFilter;
        copy.ingredientSearchRadius = bill.ingredientSearchRadius;
        copy.nextTickToSearchForIngredients = bill.nextTickToSearchForIngredients;
        copy.limitToAllowedStuff = bill.limitToAllowedStuff;
        copy.paused = bill.paused;
        copy.pauseWhenSatisfied = bill.pauseWhenSatisfied;
        copy.SetPawnRestriction(bill.PawnRestriction);
        copy.qualityRange = bill.qualityRange;
        copy.recipe = bill.recipe;
        copy.repeatCount = bill.repeatCount;
        copy.repeatMode = bill.repeatMode;
        copy.SetStoreMode(bill.GetStoreMode());
        copy.suspended = bill.suspended;
        copy.targetCount = bill.targetCount;
        copy.unpauseWhenYouHave = bill.unpauseWhenYouHave;
        return copy;
    }

    public static IEnumerable<IntVec3> FacingRect(IntVec3 pos, Rot4 dir, int range)
    {
        var rot = dir;
        rot.Rotate(RotationDirection.Clockwise);
        var xoffset = (dir.FacingCell.x * range) + dir.FacingCell.x + pos.x;
        var zoffset = (dir.FacingCell.z * range) + dir.FacingCell.z + pos.z;
        for (var x = -range; x <= range; x++)
        {
            for (var z = -range; z <= range; z++)
            {
                yield return new IntVec3(x + xoffset, pos.y, z + zoffset);
            }
        }
    }

    public static Rot4 RotateAsNew(this Rot4 rot, RotationDirection dir)
    {
        var result = rot;
        result.Rotate(dir);
        return result;
    }

    public static Option<IPlantToGrowSettable> GetPlantable(this IntVec3 pos, Map map)
    {
        if (pos.GetZone(map) is IPlantToGrowSettable value)
        {
            return Option(value);
        }

        foreach (var thing in pos.GetThingList(map))
        {
            if (thing.def.category == ThingCategory.Building && thing is IPlantToGrowSettable value2)
            {
                return Option(value2);
            }
        }

        return Nothing<IPlantToGrowSettable>();
    }

    public static bool IsAdult(this Pawn p)
    {
        return p.ageTracker.CurLifeStageIndex >= 2;
    }

    public static Color A(this Color color, float a)
    {
        var result = color;
        result.a = a;
        return result;
    }

    public static float GetEnergyAmount(ThingDef def)
    {
        return ConvertEnergyAmount(StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(def, null)));
    }

    public static float GetEnergyAmount(ThingDef def, ThingDef stuffDef)
    {
        return ConvertEnergyAmount(StatDefOf.MarketValue.Worker.GetValue(StatRequest.For(def, stuffDef)));
    }

    private static float ConvertEnergyAmount(float marketValue)
    {
        return marketValue * 0.1f;
    }
}