using System;
using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using RimWorld;
using Verse;

namespace NR_AutoMachineTool;

public class MapTickManager(Map map) : MapComponent(map)
{
    private readonly HashSet<Func<bool>> eachTickActions = [];

    private readonly Dictionary<int, HashSet<Action>> tickActionsDict = new();

    public ThingLister ThingsList { get; } = new(map);

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        Ops.ToHashSet(eachTickActions.ToList().Where(Exec)).ForEach(delegate(Func<bool> r)
        {
            eachTickActions.Remove(r);
        });
        tickActionsDict.GetOption(Find.TickManager.TicksGame).ForEach(delegate(HashSet<Action> s)
        {
            s.ToList().ForEach(Exec);
        });
        tickActionsDict.Remove(Find.TickManager.TicksGame);
    }

    private static void Exec(Action act)
    {
        try
        {
            act();
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }
    }

    private static T Exec<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
            return default;
        }
    }

    public override void MapComponentUpdate()
    {
        base.MapComponentUpdate();
        (from p in (from r in Ops.Option(Find.MainTabsRoot.OpenTab)
                    select r.TabWindow).SelectMany(w => Ops.Option(w as MainTabWindow_Architect))
                .SelectMany(a => Ops.Option(a.selectedDesPanel))
            where p.def.defName == "NR_AutoMachineTool_DesignationCategory"
            select p).ForEach(delegate { OverlayDrawHandler_UGConveyor.DrawOverlayThisFrame(); });
        if (Find.Selector.FirstSelectedObject is IBeltConbeyorLinkable)
        {
            OverlayDrawHandler_UGConveyor.DrawOverlayThisFrame();
        }
    }

    public void AfterAction(int ticks, Action act)
    {
        if (ticks < 1)
        {
            ticks = 1;
        }

        if (!tickActionsDict.TryGetValue(Find.TickManager.TicksGame + ticks, out var value))
        {
            value = [];
            tickActionsDict[Find.TickManager.TicksGame + ticks] = value;
        }

        value.Add(act);
    }

    public void NextAction(Action act)
    {
        AfterAction(1, act);
    }

    public void EachTickAction(Func<bool> act)
    {
        eachTickActions.Add(act);
    }

    public void RemoveAfterAction(Action act)
    {
        tickActionsDict.ForEach(delegate(KeyValuePair<int, HashSet<Action>> kv) { kv.Value.Remove(act); });
    }

    public void RemoveEachTickAction(Func<bool> act)
    {
        eachTickActions.Remove(act);
    }

    public bool IsExecutingThisTick(Action act)
    {
        return (from l in tickActionsDict.GetOption(Find.TickManager.TicksGame)
            select l.Contains(act)).GetOrDefault(false);
    }
}