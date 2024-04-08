using System;
using System.Collections.Generic;
using System.Linq;
using NR_AutoMachineTool.Utilities;
using UnityEngine;
using Verse;

namespace NR_AutoMachineTool;

public class Building_ItemPuller : Building_BaseLimitation<Thing>
{
    private bool active;
    private ThingFilter filter = new ThingFilter();

    protected override float SpeedFactor => Setting.pullerSetting.speedFactor;

    public override int MinPowerForSpeed => Setting.pullerSetting.minSupplyPowerForSpeed;

    public override int MaxPowerForSpeed => Setting.pullerSetting.maxSupplyPowerForSpeed;

    protected virtual int PullCount => Math.Max(Mathf.RoundToInt(SupplyPowerForSpeed / 100f), 1);

    public ThingFilter Filter => filter;

    public override Graphic Graphic => Ops.Option(base.Graphic as Graphic_Selectable).Fold(base.Graphic)(g =>
        g.Get("NR_AutoMachineTool/Buildings/Puller/Puller" + (active ? "1" : "0")));

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref filter, "filter");
        Scribe_Values.Look(ref active, "active");
        if (filter == null)
        {
            filter = new ThingFilter();
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (respawningAfterLoad)
        {
            return;
        }

        filter = new ThingFilter();
        filter.SetAllowAll(null);
    }

    protected override TargetInfo ProgressBarTarget()
    {
        return this;
    }

    private Option<Thing> TargetThing()
    {
        return (from t in (Position + Rotation.Opposite.FacingCell).SlotGroupCells(Map)
                .SelectMany(c => c.GetThingList(Map))
            where t.def.category == ThingCategory.Item
            where filter.Allows(t)
            where !IsLimit(t)
            select t).FirstOption();
    }

    public override IntVec3 OutputCell()
    {
        return Position + Rotation.FacingCell;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        var command_Toggle = new Command_Toggle
        {
            isActive = () => active,
            toggleAction = delegate { active = !active; },
            defaultLabel = "NR_AutoMachineTool_Puller.SwitchActiveLabel".Translate(),
            defaultDesc = "NR_AutoMachineTool_Puller.SwitchActiveDesc".Translate(),
            icon = RS.PlayIcon
        };
        yield return command_Toggle;
    }

    protected override bool IsActive()
    {
        return base.IsActive() && active;
    }

    protected override bool WorkInterruption(Thing working)
    {
        return !working.Spawned || working.Destroyed;
    }

    protected override bool TryStartWorking(out Thing target, out float workAmount)
    {
        target = TargetThing().GetOrDefault(null);
        workAmount = Math.Min((float?)target?.stackCount ?? 0f, PullCount) * 10f;
        return target != null;
    }

    protected override bool FinishWorking(Thing working, out List<Thing> products)
    {
        var list = new List<Thing>();
        list.Append(working.SplitOff(Math.Min(working.stackCount, PullCount)));
        products = list;
        return true;
    }
}